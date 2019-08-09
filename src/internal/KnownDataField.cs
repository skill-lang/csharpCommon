using System.Collections.Generic;

using de.ust.skill.common.csharp.@internal.parts;
using de.ust.skill.common.csharp.streams;

namespace de.ust.skill.common.csharp
{
    namespace @internal
    {

        public abstract class KnownDataField<T, Obj> : FieldDeclaration<T, Obj>, fieldDeclarations.KnownField<T, Obj> where Obj : SkillObject
        {

            protected KnownDataField(FieldType type, string name, AbstractStoragePool owner) : base(type, name, owner)
            {
            }

            /// <summary>
            /// Defer reading to rsc by creating adequate temporary simple chunks.
            /// </summary>
            public override void rbc(BulkChunk c, MappedInStream @in)
            {
                List<Block> blocks = ((AbstractStoragePool)Owner).Blocks;
                int blockIndex = 0;
                int endBlock = c.blockCount;
                while (blockIndex < endBlock)
                {
                    Block b = blocks[blockIndex++];
                    int i = b.bpo;
                    rsc(i, i + b.count, @in);
                }
            }

            /// <summary>
            /// Defer reading to osc by creating adequate temporary simple chunks.
            /// </summary>
            public override void obc(BulkChunk c)
            {
                List<Block> blocks = ((AbstractStoragePool)Owner).Blocks;
                int blockIndex = 0;
                int endBlock = c.blockCount;
                while (blockIndex < endBlock)
                {
                    Block b = blocks[blockIndex++];
                    int i = b.bpo;
                    osc(i, i + b.count);
                }
            }

            /// <summary>
            /// Defer reading to wsc by creating adequate temporary simple chunks.
            /// </summary>
            public override void wbc(BulkChunk c, MappedOutStream @out)
            {
                List<Block> blocks = ((AbstractStoragePool)Owner).Blocks;
                int blockIndex = 0;
                int endBlock = c.blockCount;
                while (blockIndex < endBlock)
                {
                    Block b = blocks[blockIndex++];
                    int i = b.bpo;
                    wsc(i, i + b.count, @out);
                }
            }
        }
    }
}