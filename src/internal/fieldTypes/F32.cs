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

            public sealed class F32 : FloatType<float>
            {

                private static readonly F32 instance = new F32();

                public static F32 get()
                {
                    return instance;
                }

                private F32() : base(12)
                {
                }

                public override object readSingleField(InStream @in)
                {
                    return @in.f32();
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
                    @out.f32(null == target ? 0 : (float)target);
                }

                public override string ToString()
                {
                    return "f32";
                }

                public override api.FieldType cast<K, V>()
                {
                    throw new System.NotImplementedException();
                }
            }
        }
    }
}