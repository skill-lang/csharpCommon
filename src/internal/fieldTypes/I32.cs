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

            public sealed class I32 : IntegerType<int>
            {

                private static readonly I32 instance = new I32();

                public static I32 get()
                {
                    return instance;
                }

                private I32() : base(9)
                {
                }

                public override object readSingleField(InStream @in)
                {
                    return @in.i32();
                }

                public override long calculateOffset(ICollection xs)
                {
                    return 4 * xs.Count;
                }

                public override long singleOffset(object x)
                {
                    return 4L;
                }

                public override void writeSingleField(object target, OutStream @out)
                {
                    @out.i32(null == target ? 0 : (int)target);
                }

                public override string ToString()
                {
                    return "i32";
                }

                public override api.FieldType cast<K, V>()
                {
                    throw new System.NotImplementedException();
                }
            }
        }
    }
}