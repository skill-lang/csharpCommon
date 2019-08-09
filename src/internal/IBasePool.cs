using System.Collections.Generic;
using System.Threading;

using de.ust.skill.common.csharp.@internal.parts;
using SkillFile = de.ust.skill.common.csharp.api.SkillFile;

namespace de.ust.skill.common.csharp
{
    namespace @internal
    {

        /// <summary>
        /// An interface to make it possible to use BasePool with an unknown type T
        /// 
        /// @author Simon Glaub
        /// </summary>
        public interface IBasePool
        {

            SkillFile Owner { get; set; }

            int getCachedSize();

            object[] getData();

            /// <summary>
            /// Allocates data and all instances for this pool and all of its sub-pools.
            /// @note invoked once upon state creation before deserialization of field data
            /// </summary>
            /// <param name="barrier">used to synchronize parallel object allocation </param>
            int performAllocations(Semaphore barrier);

            /// <summary>
            /// compress new instances into the data array and update skillIDs
            /// </summary>
            void compress(int[] lbpoMap);

            void prepareAppend(int[] lbpoMap, Dictionary<AbstractFieldDeclaration, Chunk> chunkMap);
        }
    }
}