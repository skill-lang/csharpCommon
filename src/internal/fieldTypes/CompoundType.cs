namespace de.ust.skill.common.csharp
{
    namespace @internal
    {
        namespace fieldTypes
        {

            /// <summary>
            /// Super class of all container types.
            /// 
            /// @author Simon Glaub, Timm Felden
            /// </summary>
            public abstract class CompoundType<T> : FieldType
            {
                protected CompoundType(int typeID) : base(typeID)
                {
                }
            }
        }
    }
}