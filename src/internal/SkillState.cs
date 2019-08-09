using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

using de.ust.skill.common.csharp.api;
using de.ust.skill.common.csharp.@internal.fieldTypes;
using de.ust.skill.common.csharp.streams;

namespace de.ust.skill.common.csharp
{
    namespace @internal
    {

        /// <summary>
        /// Implementation common to all skill states independent of type declarations.
        /// 
        /// @author Simon Glaub, Timm Felden
        /// </summary>
        public abstract class SkillState : SkillFile
        {

            /// <summary>
            /// if we are on windows, then we have to change some implementation details
            /// </summary>
            public static readonly bool isWindows = Environment.OSVersion.ToString().ToLower().Contains("windows");

            // types by skill name
            public readonly Dictionary<string, AbstractStoragePool> poolByName;

            public virtual AbstractStoragePool pool(string name)
            {
                return poolByName[name];
            }

            /// <summary>
            /// write mode this state is operating on
            /// </summary>
            private Mode writeMode;
            /// <summary>
            /// path that will be targeted as binary file
            /// </summary>
            private string path;
            /// <summary>
            /// a file input stream keeping the handle to a file for potential write
            /// operations
            /// 
            /// @note this is a consequence of the retarded windows file system
            /// </summary>
            private FileInputStream input;

            /// <summary>
            /// dirty flag used to prevent append after delete operations
            /// </summary>
            private bool dirty = false;

            internal readonly StringPool strings;

            /// <summary>
            /// Types required for reflective IO
            /// </summary>
            public readonly StringType stringType;
            /// <summary>
            /// Types required for reflective IO
            /// </summary>
            readonly Annotation annotationType;

            /// <summary>
            /// Path and mode management can be done for arbitrary states.
            /// </summary>
            protected SkillState(StringPool strings, string path, Mode mode, List<AbstractStoragePool> types, Dictionary<string, AbstractStoragePool> poolByName, StringType stringType, Annotation annotationType)
            {
                this.strings = strings;
                this.path = path;
                input = strings.InStream;
                writeMode = mode;
                this.types = types;
                this.poolByName = poolByName;
                this.stringType = stringType;
                this.annotationType = annotationType;
            }

            protected internal void finalizePools(FileInputStream @in)
            {
                try
                {
                    StoragePool<SkillObject, SkillObject>.establishNextPools(types);

                    // allocate instances
                    Semaphore barrier = new Semaphore(0, Int32.MaxValue);
                    {
                        int reads = 0;

                        HashSet<string> fieldNames = new HashSet<string>();
                        foreach (AbstractStoragePool p in (List<AbstractStoragePool>)allTypes())
                        {

                            // set owners
                            if (p is IBasePool)
                            {
                                ((IBasePool)p).Owner = this;
                                reads += ((IBasePool)p).performAllocations(barrier);
                            }

                            // add missing field declarations
                            fieldNames.Clear();
                            foreach (FieldDeclaration f in p.dataFields)
                            {
                                fieldNames.Add(f.Name);
                            }

                            // ensure existence of known fields
                            foreach (string n in p.knownFields)
                            {
                                if (!fieldNames.Contains(n))
                                {
                                    p.addKnownField(n, stringType, annotationType);
                                }
                            }
                        }
                        for (int i = 0; i < reads; i++)
                        {
                            barrier.WaitOne();
                        }
                    }

                    {
                        // read field data
                        int reads = 0;
                        // async reads will post their errors in this queue
                        List<SkillException> readErrors = new List<SkillException>();

                        foreach (AbstractStoragePool p in (List<AbstractStoragePool>)allTypes())
                        {
                            // @note this loop must happen in type order!

                            // read known fields
                            foreach (AbstractFieldDeclaration f in p.dataFields)
                            {
                                reads += f.finish(barrier, readErrors, @in);
                            }
                        }

                        // fix types in the Annotation-runtime type, because we need it
                        // in offset calculation
                        this.annotationType.fixTypes(poolByName);

                        // await async reads
                        for (int i = 0; i < reads; i++)
                        {
                            barrier.WaitOne();
                        }
                        foreach (SkillException e in readErrors)
                        {
                            Console.WriteLine(e.ToString());
                            Console.Write(e.StackTrace);
                        }
                        if (readErrors.Count > 0)
                        {
                            throw readErrors[0];
                        }
                    }
                }
                catch (IOException e)
                {
                    Console.WriteLine(e.ToString());
                    Console.Write(e.StackTrace);
                }
            }

            public StringAccess Strings()
            {
                return strings;
            }

            public bool contains(SkillObject target)
            {
                if (null != target)
                {
                    try
                    {
                        if (0 < target.skillID)
                        {
                            return target == poolByName[target.skillName()].getByID(target.skillID);
                        }
                        else if (0 == target.skillID)
                        {
                            return true; // will evaluate to a null pointer if stored
                        }

                        return poolByName[target.skillName()].newObjects.Contains(target);
                    }
                    catch (Exception)
                    {
                        // out of bounds or similar mean its not one of ours
                        return false;
                    }
                }
                return true;
            }

            public void delete(SkillObject target)
            {
                if (null != target)
                {
                    dirty |= target.skillID > 0;
                    poolByName[target.skillName()].delete(target);
                }
            }

            public void changePath(String path)
            {
                switch (writeMode)
                {
                    case Mode.Write:
                        break;
                    case Mode.Append:
                        // catch erroneous behavior
                        if (this.path.Equals(path))
                        {
                            return;
                        }
                        if (File.Exists(path))
                        {
                            File.Delete(path);
                        }
                        File.Copy(this.path, path);
                        break;
                    default:
                        // dead!
                        return;
                }
                this.path = path;
            }

            public string currentPath()
            {
                return path;
            }

            public void changeMode(Mode writeMode)
            {
                // pointless
                if (this.writeMode == writeMode)
                {
                    return;
                }

                switch (writeMode)
                {
                    case Mode.Write:
                        this.writeMode = writeMode;
                        goto case Mode.Append;
                    case Mode.Append:
                        // write -> append
                        throw new System.ArgumentException("Cannot change write mode from Write to Append, try to use open(<path>, Create, Append) instead.");
                    case Mode.ReadOnly:
                        throw new System.ArgumentException("Cannot change from read only, to a write mode.");

                    default:
                        // dead, if not used by DAUs
                        return;
                }
            }

            public void loadLazyData()
            {
                // ensure that strings are loaded
                int id = strings.idMap.Count;
                while (--id != 0)
                {
                    strings.get(0);
                }
                // ensure that lazy fields have been loaded
                foreach (AbstractStoragePool p in types)
                {
                    foreach (AbstractFieldDeclaration f in p.dataFields)
                    {
                        if (f is ILazyField)
                        {
                            ((ILazyField)f).ensureLoaded();
                        }
                    }
                }
            }


            public void check()
            {
                // TODO type restrictions
                // TODO make pools check fields, because they can optimize checks per
                // instance and remove redispatching, if no
                // restrictions apply anyway    
                foreach (AbstractStoragePool p in types)
                {
                    foreach (AbstractFieldDeclaration f in p.dataFields)
                    {
                        try
                        {
                            f.check();
                        }
                        catch (SkillException e)
                        {
                            throw new SkillException(string.Format("check failed in {0}.{1}:\n  {2}", p.Name, f.Name, e.Message), e);
                        }
                    }
                }

            }

            public void flush()
            {
                try
                {
                    switch (writeMode)
                    {
                        case Mode.Write:
                            if (isWindows)
                            {
                                // we have to write into a temporary file and move the file
                                // afterwards
                                string target = path;
                                string tmpPath = Path.GetTempFileName();
                                File.Delete(tmpPath);
                                FileInfo f = new FileInfo(tmpPath.Replace(".tmp", ".sf"));
                                f.Create().Close();
                                changePath(f.FullName);
                                if (input != null)
                                {
                                    input.close();
                                    input = null;
                                }
                                new StateWriter(this, FileOutputStream.write(makeInStream()));
                                if (File.Exists(target))
                                {
                                    File.Delete(target);
                                }
                                f.MoveTo(target);
                                changePath(target);
                            }
                            else
                            {
                                if (input != null)
                                {
                                    input.close();
                                    input = null;
                                }
                                new StateWriter(this, FileOutputStream.write(makeInStream()));
                            }
                            return;

                        case Mode.Append:
                            // dirty appends will automatically become writes
                            if (dirty)
                            {
                                changeMode(Mode.Write);
                                flush();
                            }
                            else
                            {
                                new StateAppender(this, FileOutputStream.append(makeInStream()));
                            }
                            return;

                        case Mode.ReadOnly:
                            throw new SkillException("Cannot flush a read only file. Note: close will turn a file into read only.");

                        default:
                            // dead
                            break;
                    }
                }
                catch (SkillException e)
                {
                    throw e;
                }
                catch (IOException e)
                {
                    throw new SkillException("failed to create or complete out stream", e);
                }
                catch (Exception e)
                {
                    throw new SkillException("unexpected exception", e);
                }
            }

            /// <returns> the file input stream matching our current status </returns>
            private FileInputStream makeInStream()
            {
                if (null == input || !path.Equals(input.Path))
                {
                    input = FileInputStream.open(path, false);
                }

                return input;
            }

            public void close()
            {
                // flush if required
                if (Mode.ReadOnly != writeMode)
                {
                    flush();
                    this.writeMode = Mode.ReadOnly;
                }

                if (null != input)
                {
                    try
                    {
                        input.close();
                    }
                    catch (IOException e)
                    {
                        // we don't care
                    }
                }
            }

            // types in type order
            protected internal readonly List<AbstractStoragePool> types;

            public IEnumerable<IAccess> allTypes()
            {
                return (List<AbstractStoragePool>)types;
            }

            public List<AbstractStoragePool> allTypesStream()
            {
                return types;
            }
        }
    }
}