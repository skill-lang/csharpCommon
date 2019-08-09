using de.ust.skill.common.csharp.@internal.parts;
using de.ust.skill.common.csharp.streams;

namespace de.ust.skill.common.csharp
{
    namespace @internal
    {
        namespace fieldDeclarations
        {
            /// <summary>
            /// An interface to make it possible to use AutoField with an unknown types T and Obj
            /// 
            /// @author Simon Glaub
            /// </summary>
            public interface IAutoField
            {
                void rsc(int i, int h, MappedInStream @in);

                void rbc(BulkChunk last, MappedInStream @in);

                void obc(BulkChunk c);

                void osc(int i, int end);

                void wbc(BulkChunk c, MappedOutStream @out);

                void wsc(int i, int end, MappedOutStream @out);
            }
        }
    }
}