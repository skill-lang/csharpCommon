namespace de.ust.skill.common.csharp
{
    namespace @internal
    {
        namespace fieldTypes
        {

            /// <summary>
            /// Mutable floats.
            /// 
            /// @author Simon Glaub, Timm Felden
            /// </summary>
            public abstract class FloatType<T> : FieldType
            {
                protected FloatType(int typeID) : base(typeID)
                {
                }
            }
        }
    }
}