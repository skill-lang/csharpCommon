using System.Collections;
using System.IO;

using FieldIterator = de.ust.skill.common.csharp.@internal.FieldIterator;
using StaticFieldIterator = de.ust.skill.common.csharp.@internal.StaticFieldIterator;

namespace de.ust.skill.common.csharp
{
    namespace api
    {
        /// <summary>
        /// An interface to make it possible to use Access with an unknown type T
        /// 
        /// @author Simon Glaub
        /// </summary>
        public interface IAccess : IGeneralAccess
        {
            /// <returns> a stream over all T's managed by this access </returns>
            Stream stream();

            /// <returns> the skill file owning this access </returns>
            SkillFile Owner { get; }

            /// <returns> the skill name of the super type, if it exists </returns>
            string superName();

            /// <returns> an iterator over fields declared by T </returns>
            StaticFieldIterator fields();

            /// <returns> an iterator over all fields of T including fields declared in super types </returns>
            FieldIterator allFields();

            /// <returns> a new T instance with default field values </returns>
            /// <exception cref="SkillException">
            ///             if no instance can be created. This is either caused by
            ///             restrictions, such as @singleton, or by invocation on unknown
            ///             types, which are implicitly unmodifiable in this
            ///             SKilL-implementation. </exception>
            object make();
        }
    }
}