using System.Collections;

using FieldIterator = de.ust.skill.common.csharp.@internal.FieldIterator;
using SkillObject = de.ust.skill.common.csharp.@internal.SkillObject;
using StaticFieldIterator = de.ust.skill.common.csharp.@internal.StaticFieldIterator;

namespace de.ust.skill.common.csharp
{
    namespace api
    {
        /// <summary>
        /// Access to class type T.
        /// 
        /// @author Simon Glaub, Timm Felden
        /// </summary>
        public interface Access<T> : GeneralAccess<T>, IAccess where T : SkillObject
        {
            /// <returns> a type ordered Container iterator over all instances of T
            /// @note do not invoke this function, if you do not know what "type order" means </returns>
            IEnumerable typeOrderIterator();

            /// <returns> an iterator over all instances of the type represented by this access not including instances of subtypes </returns>
            @internal.StaticDataIterator<T> staticInstances();

        }
    }
}