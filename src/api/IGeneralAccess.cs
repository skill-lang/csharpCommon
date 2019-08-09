using System.Collections;

namespace de.ust.skill.common.csharp
{
    namespace api
    {
        /// <summary>
        /// An interface to make it possible to use GeneralAccess with an unknown type T
        /// 
        /// @author Simon Glaub 
        /// </summary>
        public interface IGeneralAccess : IEnumerable
        {
            /// <returns> the skill name of the type </returns>
            string Name { get; }

            /// <returns> the number of objects returned by the default iterator </returns>
            int size();
        }
    }
}