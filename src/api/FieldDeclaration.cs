using SkillObject = de.ust.skill.common.csharp.@internal.SkillObject;

namespace de.ust.skill.common.csharp
{
    namespace api
    {
        /// <summary>
        /// An abstract Field declaration, used for the runtime representation of types.
        /// It can be used for reflective access of types.
        /// 
        /// @author Simon Glaub, Timm Felden 
        /// </summary>
        public abstract class FieldDeclaration
        {
            /// <returns> the skill type of this field </returns>
            public abstract FieldType Type { get; }

            /// <returns> skill name of this field </returns>
            public abstract string Name { get; }

            /// <returns> enclosing type </returns>
            public abstract IGeneralAccess Owner { get; }

            /// <summary>
            /// Generic getter for an object.
            /// </summary>
            public abstract object get(SkillObject @ref);

            /// <summary>
            /// Generic setter for an object.
            /// </summary>
            public abstract void set(SkillObject @ref, object value);
        }
    }
}