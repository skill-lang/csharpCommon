using de.ust.skill.common.csharp.streams;

namespace de.ust.skill.common.csharp
{
    namespace @internal
    {
        namespace fieldDeclarations
        {

            /// <summary>
            /// This trait marks ignored fields.
            /// 
            /// @author Simon Glaub, Timm Felden
            /// </summary>
            public abstract class IgnoredField
            {

                public void read(MappedInStream @in)
                {
                    // does nothing, the field is ignored

                    // @note maybe we have to revise this behavior for correct
                    // implementation of write
                }
            }
        }
    }
}