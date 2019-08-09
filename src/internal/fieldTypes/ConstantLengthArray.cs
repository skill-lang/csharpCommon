using System.Collections;
using System.Collections.Generic;

using de.ust.skill.common.csharp.streams;

namespace de.ust.skill.common.csharp
{
    namespace @internal
    {
        namespace fieldTypes
        {

            public sealed class ConstantLengthArray<T> : SingleArgumentType<ArrayList, T>, IConstantLengthArray
            {

                public readonly int length;

                public ConstantLengthArray(long length, FieldType groundType) : base(15, groundType)
                {
                    this.length = (int)length;
                }

                public int GetLength()
                {
                    return length;
                }

                public new FieldType GetGroundType()
                {
                    return groundType;
                }

                public override api.FieldType cast<K, V>()
                {
                    return new ConstantLengthArray<K>(this.length, this.groundType);
                }

                public override object readSingleField(InStream @in)
                {
                    ArrayList rval = new ArrayList(length);
                    for (int i = length; i-- != 0;)
                    {
                        rval.Add((T)groundType.readSingleField(@in));
                    }
                    return rval;
                }

                public override long calculateOffset(ICollection xs)
                {
                    long result = 0L;
                    foreach (ArrayList x in xs)
                    {
                        result += groundType.calculateOffset(x);
                    }

                    return result;
                }

                public override long singleOffset(object xs)
                {
                    return groundType.calculateOffset((ICollection)xs);
                }

                public override void writeSingleField(object elements, OutStream @out)
                {
                    if (((ArrayList)elements).Count != length)
                    {
                        throw new System.ArgumentException("constant length array has wrong size");
                    }

                    foreach (T e in (ArrayList)elements)
                    {
                        groundType.writeSingleField(e, @out);
                    }
                }

                public override string ToString()
                {
                    return groundType.ToString() + "[" + length + "]";
                }

                public override bool Equals(object obj)
                {
                    if (obj is IConstantLengthArray)
                    {
                        return length == ((IConstantLengthArray)obj).GetLength() && groundType.Equals(((IConstantLengthArray)obj).GetGroundType());
                    }
                    return false;
                }

            }
        }
    }
}