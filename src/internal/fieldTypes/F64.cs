using System.Collections;
using System.Collections.Generic;
using de.ust.skill.common.csharp.api;
using de.ust.skill.common.csharp.streams;

namespace de.ust.skill.common.csharp
{
    namespace @internal
    {
        namespace fieldTypes
        {

            public sealed class F64 : FloatType<double>
            {

                private static readonly F64 instance = new F64();

                public static F64 get()
                {
                    return instance;
                }

                private F64() : base(13)
                {
                }

                public override object readSingleField(InStream @in)
                {
                    return @in.f64();
                }

                public override long calculateOffset(ICollection xs)
                {
                    return 8 * xs.Count;
                }

                public override long singleOffset(object x)
                {
                    return 8L;
                }

                public override void writeSingleField(object target, OutStream @out)
                {
                    @out.f64(null == target ? 0 : (double)target);
                }

                public override string ToString()
                {
                    return "f64";
                }

                public override api.FieldType cast<K, V>()
                {
                    throw new System.NotImplementedException();
                }
            }
        }
    }
}