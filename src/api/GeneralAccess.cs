using System.Collections;

namespace de.ust.skill.common.csharp
{
    namespace api
    {
        /// <summary>
        /// Access to arbitrary skill type T including interfaces.
        /// 
        /// @author Simon Glaub, Timm Felden
        /// </summary>
        public interface GeneralAccess<T> : IEnumerable, IGeneralAccess
        {

        }
    }
}