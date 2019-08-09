using System.Collections;
using System.Collections.Generic;

using de.ust.skill.common.csharp.streams;
using de.ust.skill.common.csharp.@internal.fieldTypes;

namespace de.ust.skill.common.csharp
{
    namespace @internal
    {

        /// <summary>
        /// An interface to make it possible to use UnrootedInterfacePool with an unknown type T
        /// 
        /// @author Simon Glaub
        /// </summary>
        public interface IUnrootedInterfacePool
        {

            int size();

            IEnumerator GetEnumerator();

            string Name { get; }

            Annotation Type { get; }
        }
    }
}