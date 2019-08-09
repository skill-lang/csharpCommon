namespace de.ust.skill.common.csharp
{
    namespace @internal
    {
        namespace fieldTypes
        {

            /// <summary>
            /// An interface to make it possible to use ListType with an unknown type T
            /// 
            /// @author Simon Glaub
            /// </summary>
            public interface IListType
            {

                FieldType GetGroundType();

                string ToString();

                bool Equals(object obj);
            }
        }
    }
}