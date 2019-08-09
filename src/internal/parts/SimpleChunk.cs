namespace de.ust.skill.common.csharp
{
    namespace @internal
    {
        namespace parts
        {

            /// <summary>
            /// A chunk used for regular appearances of fields.
            /// 
            /// @author Simon Glaub, Timm Felden
            /// </summary>
            public sealed class SimpleChunk : Chunk
            {

                public readonly long bpo;

                public SimpleChunk(long begin, long end, long bpo, long count) : base(begin, end, count)
                {
                    this.bpo = bpo;
                }
            }
        }
    }
}