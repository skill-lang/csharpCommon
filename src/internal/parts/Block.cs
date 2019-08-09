namespace de.ust.skill.common.csharp
{
    namespace @internal
    {
        namespace parts
        {

            /// <summary>
            /// A block contains information about instances in a type. A StoragePool holds
            /// blocks in order of appearance in a file with the invariant, that the latest
            /// block in the list will be the latest block in the file. If a StoragePool
            /// holds no block, then it has no instances in a file.
            /// 
            /// @author Simon Glaub, Timm Felden
            /// @note While writing a Pool to disk, the latest block is the block currently
            ///       written.
            /// </summary>
            public sealed class Block
            {

                public readonly int bpo;
                public readonly int count;
                // @note cannot be file because it is calculated on resize
                public int staticCount;

                /// <param name="bpo"> the offset of the first instance </param>
                /// <param name="count"> the number of instances in this chunk </param>
                public Block(int bpo, int count, int staticCount)
                {
                    this.bpo = bpo;
                    this.count = count;
                    this.staticCount = staticCount;
                }

                /// <returns> true, iff the object with the argument skillID is inside this block </returns>
                public bool contains(long skillID)
                {
                    return bpo < skillID && skillID <= bpo + count;
                }
            }
        }
    }
}