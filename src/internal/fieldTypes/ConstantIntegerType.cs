using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using de.ust.skill.common.csharp.streams;

namespace de.ust.skill.common.csharp
{
    namespace @internal
    {
        namespace fieldTypes
        {

            /// <summary>
            /// Constant Integers.
            /// 
            /// @author Simon Glaub, Timm Felden
            /// </summary>
            public abstract class ConstantIntegerType<T> : FieldType, IConstantIntegerType
            {

                protected ConstantIntegerType(int typeID) : base(typeID)
                {
                }

                public abstract T Value { get; }

                public void expect(T arg)
                {
                    Debug.Assert(Value.Equals(arg));
                }

                public override object readSingleField(InStream @in)
                {
                    return Value;
                }

                public override long calculateOffset(ICollection xs)
                {
                    // nothing to do
                    return 0;
                }

                public override long singleOffset(object x)
                {
                    // nothing to do
                    return 0;
                }

                public override void writeSingleField(object data, OutStream @out)
                {
                    // nothing to do
                }
            }
        }
    }
}