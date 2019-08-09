using de.ust.skill.common.csharp.api;

namespace de.ust.skill.common.csharp
{
    namespace @internal
    {
        namespace fieldTypes
        {

            public sealed class ConstantI32 : ConstantIntegerType<int>
            {

                public readonly int value;

                public ConstantI32(int value) : base(2)
                {
                    this.value = value;
                }

                public override int Value
                {
                    get
                    {
                        return value;
                    }
                }

                public override string ToString()
                {
                    return string.Format("const i32 = {0:X8}", value);
                }

                public override bool Equals(object obj)
                {
                    if (obj is ConstantI32)
                    {
                        return value == ((ConstantI32)obj).value;
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