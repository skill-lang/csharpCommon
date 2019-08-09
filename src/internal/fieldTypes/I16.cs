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

            public sealed class I16 : IntegerType<short>
            {

                private static readonly I16 instance = new I16();

                public static I16 get()
                {
                    return instance;
                }

                private I16() : base(8)
                {
                }

                public override object readSingleField(InStream @in)
                {
                    return @in.i16();
                }

                public override long calculateOffset(ICollection xs)
                {
                    return 2 * xs.Count;
                }

                public override long singleOffset(object x)
                {
                    return 2L;
                }

                public override void writeSingleField(object target, OutStream @out)
                {
                    @out.i16(null == target ? (short)0 : (short)target);
                }

                public override string ToString()
                {
                    return "i16";
                }

                public override api.FieldType cast<K, V>()
                {
                    throw new System.NotImplementedException();
                }
            }
        }
    }
}