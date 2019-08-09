using System.Collections;
using System.Collections.Generic;

using de.ust.skill.common.csharp.streams;

namespace de.ust.skill.common.csharp
{
    namespace @internal
    {
        namespace fieldTypes
        {

            public sealed class VariableLengthArray<T> : SingleArgumentType<ArrayList, T>, IVariableLengthArray
            {

                public VariableLengthArray(FieldType groundType) : base(17, groundType)
                {
                }

                public override api.FieldType cast<K, V>()
                {
                    return new VariableLengthArray<K>(this.groundType);
                }

                public override object readSingleField(InStream @in)
                {
                    int i = @in.v32();
                    ArrayList rval = new ArrayList(i);
                    while (i-- != 0)
                    {
                        rval.Add((T)groundType.readSingleField(@in));
                    }
                    return rval;
                }

                public override string ToString()
                {
                    return groundType.ToString() + "[]";
                }

                public override bool Equals(object obj)
                {
                    if (obj is IVariableLengthArray)
                    {
                        return groundType.Equals(((IVariableLengthArray)obj).GetGroundType());
                    }
                    return false;
                }
            }
        }
    }
}