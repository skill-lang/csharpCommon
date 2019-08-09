using System.Collections;
using System.Collections.Generic;

using de.ust.skill.common.csharp.@internal.parts;
using de.ust.skill.common.csharp.streams;

namespace de.ust.skill.common.csharp
{
    namespace @internal
    {
        /// <summary>
        /// The fields data is distributed into an array (for now its a hash map) holding
        /// its instances.
        /// </summary>
        public class DistributedField<T, Obj> : FieldDeclaration<T, Obj> where Obj : SkillObject
        {

            public DistributedField(FieldType type, string name, AbstractStoragePool owner) : base(type, name, owner)
            {
            }

            // data held as in storage pools
            protected Dictionary<SkillObject, T> data = new Dictionary<SkillObject, T>();
            protected Dictionary<SkillObject, T> newData = new Dictionary<SkillObject, T>();

            /// <summary>
            /// Check consistency of restrictions on this field.
            /// </summary>
            public override void check()
            {
                foreach (restrictions.FieldRestriction<object> r in restrictions)
                {
                    foreach (T x in data.Values)
                    {
                        r.check(x);
                    }
                    foreach (T x in newData.Values)
                    {
                        r.check(x);
                    }
                }
            }


            public override void rsc(int i, int h, MappedInStream @in)
            {
                SkillObject[] d = (SkillObject[])((AbstractStoragePool)Owner).basePool.getData();
                for (; i != h; i++)
                {
                    data[d[i]] = (T)((FieldType)Type).readSingleField(@in);
                }
            }

            public override void rbc(BulkChunk c, MappedInStream @in)
            {
                SkillObject[] d = (SkillObject[])(((AbstractStoragePool)Owner).basePool).getData();
                List<Block> blocks = ((AbstractStoragePool)owner).Blocks;
                int blockIndex = 0;
                int endBlock = c.blockCount;
                while (blockIndex < endBlock)
                {
                    Block b = blocks[blockIndex++];
                    int i = b.bpo;
                    for (int h = i + b.count; i != h; i++)
                    {
                        data[d[i]] = (T)((FieldType)Type).readSingleField(@in);
                    }
                }
            }

            // TODO distributed fields need to be compressed as well!

            public new long offset()
            {
                Block range = ((AbstractStoragePool)Owner).lastBlock();
                // @note order is not important, because we calculate offsets only!!!
                if (range.count == data.Count)
                {
                    T[] valuesArray = new T[data.Values.Count];
                    data.Values.CopyTo(valuesArray, 0);
                    ICollection values = new List<object>();
                    foreach (object v in valuesArray)
                    {
                        ((List<object>)values).Add(v);
                    }
                    return ((FieldType)Type).calculateOffset(values);
                }

                // we have to filter the right values
                long rval = 0;
                foreach (KeyValuePair<SkillObject, T> e in data)
                {
                    if (range.contains(e.Key.skillID))
                    {
                        rval += ((FieldType)Type).singleOffset(e.Value);
                    }
                }
                return rval;
            }

            public override void osc(int i, int h)
            {
                base.offset = offset();
            }

            public override void obc(BulkChunk c)
            {
                base.offset = offset();
            }


            public override void wsc(int i, int h, MappedOutStream @out)
            {
                SkillObject[] d = (SkillObject[])((AbstractStoragePool)Owner).basePool.getData();
                for (; i < h; i++)
                {
                    ((FieldType)Type).writeSingleField(data[d[i]], @out);
                }
            }

            public override void wbc(BulkChunk c, MappedOutStream @out)
            {
                SkillObject[] d = (SkillObject[])((AbstractStoragePool)Owner).basePool.getData();
                foreach (Block bi in ((AbstractStoragePool)Owner).Blocks)
                {
                    int i = bi.bpo;
                    for (int end = i + bi.count; i < end; i++)
                    {
                        ((FieldType)Type).writeSingleField(data[d[i]], @out);
                    }
                }
            }

            public override object get(SkillObject @ref)
            {
                if (-1 == @ref.skillID)
                {
                    return newData[@ref];
                }

                return data[@ref];
            }

            public override void set(SkillObject @ref, object value)
            {
                if (-1 == @ref.skillID)
                {
                    newData[@ref] = (T)value;
                }
                else
                {
                    data[@ref] = (T)value;
                }
            }
        }
    }
}