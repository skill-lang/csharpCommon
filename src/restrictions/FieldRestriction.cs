namespace de.ust.skill.common.csharp
{
    namespace restrictions
    {

        /// <summary>
        /// A restriction that can be applied to a field.
        /// 
        /// @author Simon Glaub, Timm Felden
        /// </summary>
        public interface FieldRestriction<T> : IFieldRestriction
        {

            /// <summary>
            /// Checks a value and throws an exception in case of error. We prefer the
            /// exception throwing mechanism over return values, because we expect checks
            /// to fail almost never.
            /// </summary>
            /// <param name="value"> the value to be checked </param>
            void check(T value);
        }
    }
}