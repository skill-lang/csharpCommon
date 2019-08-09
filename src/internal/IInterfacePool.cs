using System.Collections;
using System.Collections.Generic;

using SkillFile = de.ust.skill.common.csharp.api.SkillFile;
using de.ust.skill.common.csharp.@internal.fieldTypes;
using de.ust.skill.common.csharp.streams;

namespace de.ust.skill.common.csharp
{
    namespace @internal
    {

        /// <summary>
        /// An interface to make it possible to use InterfacePool with unknown types T and B
        /// 
        /// @author Simon Glaub
        /// </summary>
        public interface IInterfacePool
        {

            int size();

            IEnumerator GetEnumerator();

            SkillFile Owner { get; }

            string Name { get; }

            string superName();

            AbstractStoragePool getSuperPool();
        }
    }
}