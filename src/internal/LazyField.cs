using System.Collections.Generic;
using System.Runtime.CompilerServices;
using de.ust.skill.common.csharp.@internal.parts;
using de.ust.skill.common.csharp.streams;

namespace de.ust.skill.common.csharp
{
    namespace @internal
    {

        /// <summary>
        /// The field is distributed and loaded on demand. Unknown fields are lazy as
        /// well.
        /// 
        /// @author Simon Glaub, Timm Felden
        /// @note implementation abuses a distributed field that can be accessed iff
        ///       there are no data chunks to be processed
        /// 
        /// @note offset and write methods will not be overwritten, because forcing has
        ///       to happen even before resetChunks
        /// </summary>
        public sealed class LazyField<T, Obj> : DistributedField<T, Obj>, ILazyField where Obj : SkillObject
        {

            public LazyField(FieldType type, string name, AbstractStoragePool owner) : base(type, name, owner)
            {
            }

            // is loaded <-> chunkMap == null
            private Dictionary<Chunk, MappedInStream> chunkMap = new Dictionary<Chunk, MappedInStream>();

            // executes pending read operations
            public void load()
            {
                foreach (KeyValuePair<Chunk, MappedInStream> p in chunkMap)
                {
                    if (p.Key.count > 0)
                    {
                        if (p.Key is BulkChunk)
                        {
                            base.rbc((BulkChunk)p.Key, p.Value);
                        }
                        else
                        {
                            Chunk c = p.Key;
                            // @note: abuse of intermediate chunk 
                            base.rsc((int)c.begin, (int)c.end, p.Value);
                        }
                    }
                }

                chunkMap = null;
            }

            // required to ensure that data is present before state reorganization
            public void ensureLoaded()
            {
                if (null != chunkMap)
                {
                    load();
                }
            }

            public override void check()
            {
                // check only, if is loaded
                if (null == chunkMap)
                {
                    base.check();
                }
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public override void rsc(int i, int h, MappedInStream @in)
            {
                chunkMap[new SimpleChunk(i, h, 1, 1)] = @in;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public override void rbc(BulkChunk target, MappedInStream @in)
            { 
                chunkMap[target] = @in;
            }

            public override object get(SkillObject @ref)
            {
                if (-1 == @ref.skillID)
                {
                    return newData[@ref];
                }

                if (null != chunkMap)
                {
                    load();
                }

                return base.get(@ref);
            }

            public override void set(SkillObject @ref, object value)
            {
                if (-1 == @ref.skillID)
                {
                    newData.Add(@ref, (T)value);
                }
                else
                {
                    if (null != chunkMap)
                    {
                        load();
                    }

                    base.set(@ref, value);
                }
            }
        }
    }
}