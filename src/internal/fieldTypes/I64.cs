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

            public sealed class I64 : IntegerType<long>
            {
                private static readonly I64 instance = new I64();

                public static I64 get()
                {
                    return instance;
                }

                private I64() : base(10)
                {
                }

                public override object readSingleField(InStream @in)
                {
                    return @in.i64();
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
                    @out.i64(null == target ? 0 : (long)target);
                }

                public override string ToString()
                {
                    return "i64";
                }

                public override api.FieldType cast<K, V>()
                {
                    throw new System.NotImplementedException();
                }
            }
        }
    }
}