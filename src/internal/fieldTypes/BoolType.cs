using System.Collections;
using System.Collections.Generic;

using de.ust.skill.common.csharp.streams;

namespace de.ust.skill.common.csharp
{
    namespace @internal
    {
        namespace fieldTypes
        {

            public sealed class BoolType : FieldType
            {

                private static readonly BoolType instance = new BoolType();

                public static BoolType get()
                {
                    return instance;
                }

                private BoolType() : base(6)
                {
                }

                public override object readSingleField(InStream @in)
                {
                    return @in.@bool();
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
                    @out.@bool(null != target && (bool)target);
                }

                public override string ToString()
                {
                    return "bool";
                }

                public override api.FieldType cast<K, V>()
                {
                    throw new System.NotImplementedException();
                }
            }
        }
    }
}