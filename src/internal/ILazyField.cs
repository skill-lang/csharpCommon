using de.ust.skill.common.csharp.@internal.parts;
using de.ust.skill.common.csharp.streams;

namespace de.ust.skill.common.csharp
{
    namespace @internal
    {

        /// <summary>
        /// An interface to make it possible to use LazyField with unknown types T and Obj
        /// 
        /// @author Simon Glaub
        /// </summary>
        public interface ILazyField
        {

            // executes pending read operations
            void load();

            // required to ensure that data is present before state reorganization
            void ensureLoaded();

            void check();

            void rsc(int i, int h, MappedInStream @in);

            void rbc(BulkChunk target, MappedInStream @in);
        }
    }
}