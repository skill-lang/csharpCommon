namespace de.ust.skill.common.csharp
{
    namespace @internal
    {
        namespace fieldTypes
        {

            /// <summary>
            /// Mutable integers.
            /// 
            /// @author Simon Glaub, Timm Felden
            /// </summary>
            public abstract class IntegerType<T> : FieldType
            {
                protected IntegerType(int typeID) : base(typeID)
                {
                }
            }
        }
    }
}