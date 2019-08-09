using SkillException = de.ust.skill.common.csharp.api.SkillException;

namespace de.ust.skill.common.csharp
{
    namespace restrictions
    {

        /// <summary>
        /// A nonnull restricition. It will ensure that field data is non null.
        /// 
        /// @author Simon Glaub, Timm Felden
        /// </summary>
        public class NonNull : FieldRestriction<object>
        {

            private static readonly NonNull instance = new NonNull();

            private NonNull()
            {
            }

            public static NonNull get()
            {
                return instance;
            }

            public void check(object value)
            {
                if (value == null)
                {
                    throw new SkillException("Null value violates @NonNull.");
                }
            }
        }
    }
}