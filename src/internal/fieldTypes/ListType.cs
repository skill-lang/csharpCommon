using System.Collections.Generic;

using de.ust.skill.common.csharp.streams;

namespace de.ust.skill.common.csharp
{
    namespace @internal
    {
        namespace fieldTypes
        {

            public sealed class ListType<T> : SingleArgumentType<List<T>, T>, IListType
            {

                public ListType(FieldType groundType) : base(18, groundType)
                {
                }

                public new FieldType GetGroundType()
                {
                    return groundType;
                }

                public override api.FieldType cast<K, V>()
                {
                    return new ListType<K>(this.groundType);
                }

                public override object readSingleField(InStream @in)
                {
                    List<T> rval = new List<T>();
                    for (int i = @in.v32(); i != 0; i--)
                    {
                        rval.Add((T)groundType.readSingleField(@in));
                    }
                    return rval;
                }

                public override string ToString()
                {
                    return "list<" + groundType.ToString() + ">";
                }

                public override bool Equals(object obj)
                {
                    if (obj is IListType)
                    {
                        return groundType.Equals(((IListType)obj).GetGroundType());
                    }
                    return false;
                }
            }
        }
    }
}