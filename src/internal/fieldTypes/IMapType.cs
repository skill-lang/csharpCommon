namespace de.ust.skill.common.csharp
{
    namespace @internal
    {
        namespace fieldTypes
        {

            /// <summary>
            /// An interface to make it possible to use MapType with unknown types K and V
            /// 
            /// @author Simon Glaub
            /// </summary>
            public interface IMapType
            {
                FieldType GetKeyType();

                FieldType GetValueType();

                string ToString();

                bool Equals(object obj);
            }
        }
    }
}