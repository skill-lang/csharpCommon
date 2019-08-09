using de.ust.skill.common.csharp.api;

namespace de.ust.skill.common.csharp
{
    namespace @internal
    {
        namespace fieldTypes
        {

            public sealed class ConstantI8 : ConstantIntegerType<sbyte>
            {

                public readonly sbyte value;

                public ConstantI8(sbyte value) : base(0)
                {
                    this.value = value;
                }

                public override sbyte Value
                {
                    get
                    {
                        return value;
                    }
                }

                public override string ToString()
                {
                    return string.Format("const i8 = {0:X2}", value);
                }

                public override bool Equals(object obj)
                {
                    if (obj is ConstantI8)
                    {
                        return value == ((ConstantI8)obj).value;
                    }
                    return false;
                }

                public override api.FieldType cast<K, V>()
                {
                    throw new System.NotImplementedException();
                }
            }
        }
    }
}