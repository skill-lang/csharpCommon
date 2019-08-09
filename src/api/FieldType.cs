namespace de.ust.skill.common.csharp
{
    namespace api
    {
        /// <summary>
        /// Field types as used in reflective access.
        /// 
        /// @author Simon Glaub, Timm Felden 
        public abstract class FieldType
        {
            /// <returns> the ID of this type (respective to the state in which it lives) </returns>
            public abstract int TypeID { get; }

            /// <returns> a human readable and unique representation of this type </returns>
            public abstract override string ToString();

            /// <summary>
            /// Cast Generic FieldTypes
            /// 
            /// @note V is only needed for MapType
            /// </summary>
            public abstract FieldType cast<K, V>();
        }
    }
}