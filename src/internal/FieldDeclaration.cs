using System;
using System.Threading;
using System.Collections.Generic;

using SkillException = de.ust.skill.common.csharp.api.SkillException;
using IGeneralAccess = de.ust.skill.common.csharp.api.IGeneralAccess;
using de.ust.skill.common.csharp.@internal.exceptions;
using de.ust.skill.common.csharp.@internal.fieldDeclarations;
using de.ust.skill.common.csharp.@internal.parts;
using de.ust.skill.common.csharp.restrictions;
using de.ust.skill.common.csharp.streams;

namespace de.ust.skill.common.csharp
{
    namespace @internal
    {

        /// <summary>
        /// Actual implementation as used by all bindings.
        /// 
        /// @author Simon Glaub, Timm Felden
        /// </summary>
        public abstract class FieldDeclaration<T, Obj> : AbstractFieldDeclaration where Obj : SkillObject
        {

            public override api.FieldType Type
            {
                get
                {
                    return type;
                }
            }


            public override string Name
            {
                get
                {
                    return name;
                }
            }

            /// <summary>
            /// Restriction handling.
            /// </summary>
            public readonly HashSet<IFieldRestriction> restrictions = new HashSet<IFieldRestriction>();

            public override void addRestriction(IFieldRestriction r)
            {
                restrictions.Add(r);
            }

            /// <summary>
            /// Check consistency of restrictions on this field.
            /// </summary>
            public override void check()
            {
                if (restrictions.Count > 0)
                {
                    foreach (Obj x in owner)
                    {
                        if (!x.isDeleted())
                        {
                            foreach (IFieldRestriction r in restrictions)
                            {
                                if (r is NonNull)
                                {
                                    ((NonNull)r).check((T)get(x));
                                }
                                else
                                {
                                    ((FieldRestriction<T>)r).check((T)get(x));
                                }
                            }
                        }
                    }
                }
            }

            public override IGeneralAccess Owner
            {
                get
                {
                    return (IGeneralAccess)owner;
                }
            }

            /// <summary>
            /// regular field constructor
            /// </summary>
            protected FieldDeclaration(FieldType type, string name, AbstractStoragePool owner) : base(type, name, owner)
            {
            }

            /// <summary>
            /// auto field constructor
            /// </summary>
            protected FieldDeclaration(FieldType type, string name, int index, AbstractStoragePool owner) : base(type, name, index, owner)
            {

            }

            public override string ToString()
            {
                return type.ToString() + " " + name;
            }

            /// <summary>
            /// Field declarations are equal, iff their names and types are equal.
            /// 
            /// @note This makes fields of unequal enclosing types equal!
            /// </summary>
            public override sealed bool Equals(object obj)
            {
                if (this == obj)
                {
                    return true;
                }
                if (obj is AbstractFieldDeclaration)
                {
                    return ((AbstractFieldDeclaration)obj).Name.Equals(name) && ((AbstractFieldDeclaration)obj).type.Equals(type);
                }

                return false;
            }

            public override int GetHashCode()
            {
                return type.GetHashCode() ^ name.GetHashCode();
            }

            public override void addChunk(Chunk chunk)
            {
                dataChunks.Add(chunk);
            }

            /// <summary>
            /// Make offsets absolute.
            /// </summary>
            /// <returns> the end of this chunk </returns>
            public override long addOffsetToLastChunk(FileInputStream @in, long offset)
            {
                Chunk c = lastChunk();
                c.begin += offset;
                c.end += offset;

                return c.end;
            }

            public override Chunk lastChunk()
            {
                return dataChunks[dataChunks.Count - 1];
            }

            /// <summary>
            /// reset Chunks before writing a file
            /// </summary>
            public override void resetChunks(int lbpo, int newSize)
            {
                dataChunks.Clear();
                dataChunks.Add(new SimpleChunk(-1, -1, lbpo, newSize));
            }

            /// <summary>
            /// Coordinates reads and prevents from state corruption using the barrier.
            /// </summary>
            /// <param name="barrier"> takes one permit in the caller thread and returns one in the
            ///            reader thread (per block) </param>
            /// <param name="readErrors"> errors will be reported in this queue </param>
            /// <returns> number of jobs started </returns>
            public override int finish(Semaphore barrier, List<SkillException> readErrors, FileInputStream @in)
            {
                // skip lazy and ignored fields
                if (this is IgnoredField)
                {
                    return 0;
                }

                int block = 0;
                foreach (Chunk c in dataChunks)
                {
                    int blockCounter = block++;
                    FieldDeclaration<T, Obj> f = this;

                    //ThreadPool.QueueUserWorkItem(new WaitCallback((Object stateInfo) =>
                    //{
                        SkillException ex = null;
                        try
                        {
                            // check that map was fully consumed and remove it
                            MappedInStream map = @in.map(0L, c.begin, c.end);
                            if (c is BulkChunk)
                            {
                                f.rbc((BulkChunk)c, map);
                            }
                            else
                            {
                                int i = (int)((SimpleChunk)c).bpo;
                                f.rsc(i, i + (int)c.count, map);
                            }

                            if (!map.eof() && !(f is ILazyField))
                            {
                                ex = new PoolSizeMissmatchError(blockCounter, map.position(), c.begin, c.end, f);
                            }

                        }
                        catch (InvalidOperationException e)
                        {
                            ex = new PoolSizeMissmatchError(blockCounter, c.begin, c.end, f, e);
                        }
                        catch (SkillException t)
                        {
                            ex = t;
                        }
                        catch (Exception t)
                        {
                            ex = new SkillException("internal error: unexpected foreign exception", t);
                        }
                        finally
                        {
                            barrier.Release();
                            if (null != ex)
                            {
                                lock (readErrors)
                                {
                                    readErrors.Add(ex);
                                }
                            }
                        }
                    //}));
                }
                return block;
            }

        }
    }
}