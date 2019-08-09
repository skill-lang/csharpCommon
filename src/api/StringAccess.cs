using System.Collections.Generic;

namespace de.ust.skill.common.csharp
{
    namespace api
    {
        /// <summary>
        /// Provides access to Strings in the pool.
        /// 
        /// @note As it is the case with Strings in C#, Strings in SKilL are special
        ///       objects that behave slightly different, because they are something in
        ///       between numbers and objects.
        /// @author Simon Glaub, Timm Felden
        /// </summary>
        public interface StringAccess : ICollection<string>
        {
            /// <summary>
            /// get String by its Skill ID
            /// </summary>
            string get(int index);
        }
    }
}