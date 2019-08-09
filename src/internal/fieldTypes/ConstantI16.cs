namespace de.ust.skill.common.csharp
{
    namespace @internal
    {
        namespace fieldTypes
        {
            public sealed class ConstantI16 : ConstantIntegerType<short>
            {

                public readonly short value;

                public ConstantI16(short value) : base(1)
                {
                    this.value = value;
                }

                public override short Value
                {
                    get
                    {
                        return value;
                    }
                }

                public override string ToString()
                {
                    return string.Format("const i16 = {0:X4}", value);
                }

                public override bool Equals(object obj)
                {
                    if (obj is ConstantI16)
                    {
                        return value == ((ConstantI16)obj).value;
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