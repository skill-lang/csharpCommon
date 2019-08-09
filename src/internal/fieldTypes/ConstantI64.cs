using de.ust.skill.common.csharp.api;

namespace de.ust.skill.common.csharp
{
    namespace @internal
    {
        namespace fieldTypes
        {

            public sealed class ConstantI64 : ConstantIntegerType<long>
            {

                public readonly long value;

                public ConstantI64(long value) : base(3)
                {
                    this.value = value;
                }

                public override long Value
                {
                    get
                    {
                        return value;
                    }
                }

                public override string ToString()
                {
                    return string.Format("const i64 = {0:X16}", value);
                }

                public override bool Equals(object obj)
                {
                    if (obj is ConstantI64)
                    {
                        return value == ((ConstantI64)obj).value;
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