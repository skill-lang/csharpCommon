using System.Collections.Generic;

using de.ust.skill.common.csharp.streams;

namespace de.ust.skill.common.csharp
{
    namespace @internal
    {
        namespace fieldTypes
        {

            public sealed class SetType<T> : SingleArgumentType<HashSet<T>, T>, ISetType
            {

                public SetType(FieldType groundType) : base(19, groundType)
                {
                }

                public new FieldType GetGroundType()
                {
                    return groundType;
                }

                public override api.FieldType cast<K, V>()
                {
                    return new SetType<K>(this.groundType);
                }

                public override object readSingleField(InStream @in)
                {
                    int i = @in.v32();
                    HashSet<T> rval = new HashSet<T>();
                    while (i-- != 0)
                    {
                        rval.Add((T)groundType.readSingleField(@in));
                    }
                    return rval;
                }

                public override string ToString()
                {
                    return "set<" + groundType.ToString() + ">";
                }

                public override bool Equals(object obj)
                {
                    if (obj is ISetType)
                    {
                        return groundType.Equals(((ISetType)obj).GetGroundType());
                    }
                    return false;
                }
            }
        }
    }
}