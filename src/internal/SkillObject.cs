using System;
using System.Text;

using SkillFile = de.ust.skill.common.csharp.api.SkillFile;

namespace de.ust.skill.common.csharp
{
    namespace @internal
    {
        /// <summary>
        /// The root of the hierarchy of instances of skill user types. Annotations can
        /// store arbitrary objects, thus this type has to exist explicitly.
        /// 
        /// @author Simon Glaub, Timm Felden
        /// @note This type definition is in internal, because we have to protect
        ///       setSkillID from the user
        /// </summary>
        // TODO create a builder for skill objects
        [Serializable]
        public abstract class SkillObject
        {

            /// <summary>
            /// @note For change-tolerant serialization use SKilL. This
            ///       mechanism has been implemented for the purpose of comparison only.
            /// </summary>
            private const long serialVersionUID = unchecked((long)0xDecaFade00005c11L);

            /// <summary>
            /// The constructor is protected to ensure that users do not break states
            /// accidentally
            /// </summary>
            protected SkillObject(int skillID)
            {
                this.skillID = skillID;
            }

            /// <summary>
            /// -1 for new objects br
            /// 0 for deleted objects br
            /// everything else is the ID of an object inside of a file
            /// </summary>
            [NonSerialized]
            protected internal int skillID;

            /// <returns> whether the object has been deleted </returns>
            public virtual bool isDeleted()
            {
                return 0 == skillID;
            }

            /// <summary>
            /// Do not rely on skill ID if you do not know exactly what you are doing.
            /// </summary>
            public int SkillID
            {
                get
                {
                    return skillID;
                }
            }

            /// <returns> the skill name of this type </returns>
            public abstract string skillName();

            /// <summary>
            /// reflective setter
            /// </summary>
            /// <param name="field"> a field declaration instance as obtained from the storage
            ///            pools iterator </param>
            /// <param name="value"> the new value of the field
            /// @note if field is not a field of this.type, then anything may happen </param>
            public void set(api.FieldDeclaration field, object value)
            {
                field.set(this, value);
            }

            /// <summary>
            /// reflective getter
            /// </summary>
            /// <param name="field"> a field declaration instance as obtained from the storage pools iterator
            /// @note if field is not a field of this.type, then anything may happen
            public object get(api.FieldDeclaration field)
            {
                return field.get(this);
            }

            /// <summary>
            /// potentially expensive but more pretty representation of this instance.
            /// </summary>
            public string prettyString(SkillFile sf)
            {
                StringBuilder sb = (new StringBuilder("Age(this: ")).Append(this);
                AbstractStoragePool p = ((SkillState)sf).poolByName[skillName()];
                printFs(p.allFields(), sb);
                return sb.Append(")").ToString();
            }

            // provides required extra type quantification
            private void printFs(FieldIterator fieldIterator, StringBuilder sb)
            {
                while (fieldIterator.hasNext())
                {
                    AbstractFieldDeclaration f = fieldIterator.next();
                    sb.Append(", ").Append(f.Name).Append(": ").Append(f.get(this));
                }
            }

            [Serializable]
            public sealed class SubType : SkillObject, NamedType
            {
                /// <summary>
                /// should not happen
                /// </summary>
                internal new const long serialVersionUID = 3283783094243102233L;

                [NonSerialized]
                readonly AbstractStoragePool τPool;

                public SubType(AbstractStoragePool τPool, int skillID) : base(skillID)
                {
                    this.τPool = τPool;
                }

                public AbstractStoragePool ΤPool
                {
                    get
                    {
                        return τPool;
                    }
                }

                public override string ToString()
                {
                    return skillName() + "#" + skillID;
                }

                public override string skillName()
                {
                    return τPool.Name;
                }
            }
        }
    }
}