using SkillException = de.ust.skill.common.csharp.api.SkillException;
using FieldType = de.ust.skill.common.csharp.api.FieldType;

namespace de.ust.skill.common.csharp
{
    namespace @internal
    {
        namespace exceptions
        {

            /// <summary>
            /// Thrown in case of a type miss-match on a field type.
            /// 
            /// @author Simon Glaub, Timm Felden
            /// </summary>
            public class TypeMissmatchError : SkillException
            {

                public TypeMissmatchError(FieldType type, string expected, string field, string pool) : base(string.Format("During construction of {0}.{1}: Encountered incompatible type \"{2}\" (expected: {3})", pool, field, type.ToString(), expected))
                {
                }

            }
        }
    }
}