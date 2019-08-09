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

            public sealed class I8 : IntegerType<sbyte>
            {

                private static readonly I8 instance = new I8();

                public static I8 get()
                {
                    return instance;
                }

                private I8() : base(7)
                {
                }

                public override object readSingleField(InStream @in)
                {
                    return @in.i8();
                }

                public override long calculateOffset(ICollection xs)
                {
                    return xs.Count;
                }

                public override long singleOffset(object x)
                {
                    return 1L;
                }

                public override void writeSingleField(object target, OutStream @out)
                {
                    @out.i8(null == target ? (sbyte)0 : (sbyte)target);
                }

                public override string ToString()
                {
                    return "i8";
                }

                public override api.FieldType cast<K, V>()
                {
                    throw new System.NotImplementedException();
                }
            }
        }
    }
}