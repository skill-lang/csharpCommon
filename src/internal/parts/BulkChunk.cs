namespace de.ust.skill.common.csharp
{
    namespace @internal
    {
        namespace parts
        {

            /// <summary>
            /// A chunk that is used iff a field is appended to a preexisting type in a
            /// block.
            /// 
            /// @author Simon Glaub, Timm Felden
            /// </summary>
            public class BulkChunk : Chunk
            {

                /// <summary>
                /// number of blocks represented by this chunk
                /// </summary>
                public readonly int blockCount;

                public BulkChunk(long begin, long end, long count, int blockCount) : base(begin, end, count)
                {
                    this.blockCount = blockCount;
                }
            }
        }
    }
}