using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;

using SkillException = de.ust.skill.common.csharp.api.SkillException;
using de.ust.skill.common.csharp.@internal.fieldTypes;
using de.ust.skill.common.csharp.@internal.parts;
using de.ust.skill.common.csharp.streams;
using System.Collections;

namespace de.ust.skill.common.csharp
{
    namespace @internal
    {

        /// <summary>
        /// Provides serialization functions;
        /// @author Timm Felden
        /// </summary>
        /// <seealso cref="SKilL §6.4"/>
        public abstract class SerializationFunctions
        {

            /// <summary>
            /// Data structure used for parallel serialization scheduling
            /// </summary>
            protected sealed class Task
            {
                public readonly AbstractFieldDeclaration f;
                public readonly long begin;
                public readonly long end;
                public MappedOutStream outMap;
                public LinkedList<SkillException> writeErrors;
                public Semaphore barrier;

                internal Task(AbstractFieldDeclaration f, long begin, long end)
                {
                    this.f = f;
                    this.begin = begin;
                    this.end = end;
                }

                public void run()
                {
                    try
                    {
                        Chunk c = f.lastChunk();
                        if (c is SimpleChunk)
                        {
                            int i = (int)((SimpleChunk)c).bpo;
                            f.wsc(i, i + (int)c.count, outMap);
                        }
                        else
                        {
                            f.wbc((BulkChunk)c, outMap);
                        }

                    }
                    catch (SkillException e)
                    {
                        lock (writeErrors)
                        {
                            writeErrors.AddLast(e);
                        }
                    }
                    catch (IOException e)
                    {
                        lock (writeErrors)
                        {
                            writeErrors.AddLast(new SkillException("failed to write field " + f.ToString(), e));
                        }
                    }
                    catch (Exception e)
                    {
                        lock (writeErrors)
                        {
                            writeErrors.AddLast(new SkillException("unexpected failure while writing field " + f.ToString(), e));
                        }
                    }
                    finally
                    {
                        // ensure that writer can terminate, errors will be
                        // printed to command line anyway, and we wont
                        // be able to recover, because errors can only happen if
                        // the skill implementation itself is
                        // broken
                        //outMap.close();
                        barrier.Release();
                    }
                }
            }

            protected readonly SkillState state;
            internal readonly Dictionary<string, int> stringIDs;

            public SerializationFunctions(SkillState state)
            {
                this.state = state;

                /// <summary>
                /// @note this is a O(σ) operation:)
                /// @note we do no longer make use of the generation time type info,
                ///       because we want to treat generic fields as well
                /// </summary>
                StringPool strings = (StringPool)state.Strings();

                foreach (AbstractStoragePool p in state.types)
                {
                    strings.Add(p.Name);
                    foreach (AbstractFieldDeclaration f in p.dataFields)
                    {

                        strings.Add(f.Name);
                        // collect strings
                        switch (f.Type.TypeID)
                        {
                            // string
                            case 14:
                                foreach (SkillObject i in p)
                                {
                                    if (!i.isDeleted())
                                    {
                                        strings.Add((string)i.get(f));
                                    }
                                }
                                break;

                            // container<string>
                            case 15:
                            case 17:
                            case 18:
                            case 19:
                                if (((ISingleArgumentType)(f.Type)).GetGroundType().TypeID == 14)
                                {
                                    foreach (SkillObject i in p)
                                    {
                                        if (!i.isDeleted())
                                        {
                                            ICollection<string> xs = (ICollection<string>)i.get(f);
                                            foreach (string s in xs)
                                            {
                                                strings.Add(s);
                                            }
                                        }
                                    }
                                }
                                break;

                            case 20:
                                IMapType type = (IMapType)f.Type;
                                // simple maps
                                bool k, v;
                                if ((k = (type.GetKeyType().TypeID == 14)) | (v = (type.GetValueType().TypeID == 14)))
                                {

                                    foreach (SkillObject i in p)
                                    {

                                        if (!i.isDeleted())
                                        {
                                            IDictionary xs = (IDictionary)i.get(f);
                                            if (null != xs)
                                            {
                                                if (k)
                                                {
                                                    foreach (string s in (xs).Keys)
                                                    {
                                                        strings.Add(s);
                                                    }
                                                }
                                                if (v)
                                                {
                                                    foreach (string s in xs.Values)
                                                    {
                                                        strings.Add(s);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                // @note overlap is intended
                                // nested maps
                                if (type.GetValueType().TypeID == 20)
                                {
                                    IMapType nested = (IMapType)type.GetValueType();
                                    if (nested.GetKeyType().TypeID == 14 || nested.GetValueType().TypeID == 14 || nested.GetValueType().TypeID == 20)
                                    {
                                        foreach (SkillObject i in p)
                                        {
                                            if (!i.isDeleted())
                                            {
                                                collectNestedStrings(strings, type, (IDictionary)i.get(f));
                                            }
                                        }
                                    }
                                }
                                break;

                            default:
                                // nothing important
                                break;
                        }

                        /// <summary>
                        /// ensure that lazy fields have been loaded
                        /// </summary>
                        if (f is ILazyField)
                        {
                            ((ILazyField)f).ensureLoaded();
                        }
                    }
                }

                /// <summary>
                /// check consistency of the state, now that we aggregated all instances
                /// </summary>
                state.check();

                stringIDs = state.stringType.resetIDs();
            }

            private static void collectNestedStrings(StringPool strings, IMapType type, IDictionary xs)
            {
                if (null != xs)
                {
                    if (type.GetKeyType().TypeID == 14)
                    {
                        foreach (string s in xs.Keys)
                        {
                            strings.Add(s);
                        }
                    }

                    if (type.GetValueType().TypeID == 14)
                    {
                        foreach (string s in xs.Values)
                        {
                            strings.Add(s);
                        }
                    }

                    if (type.GetValueType().TypeID == 20)
                    {
                        foreach (IDictionary s in xs.Values)
                        {
                            collectNestedStrings(strings, (IMapType)type.GetValueType(), s);
                        }
                    }
                }
            }

            /// <summary>
            /// TODO serialization of restrictions
            /// </summary>
            protected static void restrictions(AbstractStoragePool p, OutStream @out)
            {
                @out.i8((sbyte)0);
            }

            /// <summary>
            /// TODO serialization of restrictions
            /// </summary>
            protected internal static void restrictions(AbstractFieldDeclaration f, OutStream @out)
            {
                @out.i8((sbyte)0);
            }

            /// <summary>
            /// serialization of types is fortunately independent of state, because field
            /// types know their ID
            /// </summary>
            protected internal static void writeType(FieldType t, OutStream @out)
            {
                switch (t.TypeID)
                {
                    // case ConstantI8(v) ⇒
                    case 0:
                        @out.i8((sbyte)0);
                        @out.i8(((ConstantI8)t).Value);
                        return;

                    // case ConstantI16(v) ⇒
                    case 1:
                        @out.i8((sbyte)1);
                        @out.i16(((ConstantI16)t).Value);
                        return;

                    // case ConstantI32(v) ⇒
                    case 2:
                        @out.i8((sbyte)2);
                        @out.i32(((ConstantI32)t).Value);
                        return;

                    // case ConstantI64(v) ⇒
                    case 3:
                        @out.i8((sbyte)3);
                        @out.i64(((ConstantI64)t).Value);
                        return;

                    // case ConstantV64(v) ⇒
                    case 4:
                        @out.i8((sbyte)4);
                        @out.v64(((ConstantV64)t).Value);
                        return;

                    // case ConstantLengthArray(l, t) ⇒
                    case 15:
                        @out.i8((sbyte)0x0F);
                        @out.v64(((IConstantLengthArray)t).GetLength());
                        @out.v64(((ISingleArgumentType)t).GetGroundType().TypeID);
                        return;

                    // case VariableLengthArray(t) ⇒
                    // case ListType(t) ⇒
                    // case SetType(t) ⇒
                    case 17:
                    case 18:
                    case 19:
                        @out.i8((sbyte)t.TypeID);
                        @out.v64(((ISingleArgumentType)t).GetGroundType().TypeID);
                        return;

                    // case MapType(k, v) ⇒
                    case 20:
                        @out.i8((sbyte)0x14);
                        writeType(((IMapType)t).GetKeyType(), @out);
                        writeType(((IMapType)t).GetValueType(), @out);
                        return;

                    default:
                        @out.v64(t.TypeID);
                        return;
                }
            }

            protected static void writeFieldData(SkillState state, FileOutputStream @out, List<Task> data, int offset, Semaphore barrier)
            {

                // async reads will post their errors in this queue
                LinkedList<SkillException> writeErrors = new LinkedList<SkillException>();

                MappedOutStream writeMap = @out.mapBlock(offset);
                foreach (Task t in data)
                {
                    t.outMap = writeMap.clone((int)t.begin, (int)t.end);
                    t.writeErrors = writeErrors;
                    t.barrier = barrier;
                    ThreadPool.QueueUserWorkItem(new WaitCallback((Object stateInfo) => t.run()));
                }
                for (int i = 0; i < data.Count; i++)
                {
                    barrier.WaitOne();
                }
                writeMap.close();
                @out.close();

                // report errors
                foreach (SkillException e in writeErrors)
                {
                    Console.WriteLine(e.ToString());
                    Console.Write(e.StackTrace);
                }
                if (writeErrors.Count > 0)
                {
                    throw writeErrors.First.Value;
                }

                /// <summary>
                /// **************** PHASE 4: CLEANING * ****************
                /// </summary>
                // release data structures
                state.stringType.clearIDs();
                StoragePool<SkillObject, SkillObject>.unfix(state.types);
            }
        }
    }
}