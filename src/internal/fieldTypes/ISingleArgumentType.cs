namespace de.ust.skill.common.csharp
{
    namespace @internal
    {
        namespace fieldTypes
        {

            /// <summary>
            /// An interface to make it possible to use SingleArgumentType with unknown type T
            /// 
            /// @author Simon Glaub
            /// </summary>
            public interface ISingleArgumentType
            {

                FieldType GetGroundType();

            }
        }
    }
}