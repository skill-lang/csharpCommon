using System.Collections;

using de.ust.skill.common.csharp.@internal.parts;

namespace de.ust.skill.common.csharp
{
    namespace @internal
    {

        /// <summary>
        /// An interface to make it possible to use DynamiocDataIterator with unknown types T and B
        /// 
        /// @author Simon Glaub
        /// </summary>
        public interface IDynamicDataIterator
        {

            bool hasNext();

            object next();

            void nextP();

            IEnumerator GetEnumerator();
        }
    }
}