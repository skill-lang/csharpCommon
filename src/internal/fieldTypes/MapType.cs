using System.Collections;
using System.Collections.Generic;
using System.Text;

using de.ust.skill.common.csharp.streams;

namespace de.ust.skill.common.csharp
{
    namespace @internal
    {
        namespace fieldTypes
        {

            public sealed class MapType<K, V> : CompoundType<Dictionary<K, V>>, IMapType
            {

                public readonly FieldType keyType;
                public readonly FieldType valueType;

                public MapType(FieldType keyType, FieldType valueType) : base(20)
                {
                    this.keyType = keyType;
                    this.keyType = keyType;
                    this.valueType = valueType;
                }

                public FieldType GetKeyType()
                {
                    return keyType;
                }

                public FieldType GetValueType()
                {
                    return valueType;
                }

                public override api.FieldType cast<S, T>()
                {
                    return new MapType<S, T>(this.keyType, this.valueType);
                }

                public override object readSingleField(InStream @in)
                {
                    int i = @in.v32();
                    Dictionary<K, V> rval = new Dictionary<K, V>(1 + (i * 3) / 2);
                    while (i-- != 0)
                    {
                        rval[(K)keyType.readSingleField(@in)] = (V)valueType.readSingleField(@in);
                    }
                    return rval;
                }

                public override long calculateOffset(ICollection xs)
                {
                    long result = 0L;
                    foreach (IDictionary x in xs)
                    {
                        int size = x.Count;
                        if (0 == size)
                        {
                            result++;
                        }
                        else
                        {
                            result += V64.singleV64Offset(size) + keyType.calculateOffset(x.Keys) + valueType.calculateOffset(x.Values);
                        }
                    }

                    return result;
                }

                public override long singleOffset(object x)
                {
                    int size = null == x ? 0 : ((Dictionary<K, V>)x).Count;
                    if (0 == size)
                    {
                        return 1L;
                    }

                    return V64.singleV64Offset(((Dictionary < K, V >)x).Count) + keyType.calculateOffset(((Dictionary < object, V >)x).Keys) + valueType.calculateOffset(((Dictionary<K, object>)x).Values);
                }

                public override void writeSingleField(object data, OutStream @out)
                {
                    int size = (null == data) ? 0 : ((Dictionary < K, V >)data).Count;
                    if (0 == size)
                    {
                        @out.i8((sbyte)0);
                        return;
                    }
                    @out.v64(size);
                    foreach (KeyValuePair<K, V> e in ((Dictionary<K, V>)data))
                    {
                        keyType.writeSingleField(e.Key, @out);
                        valueType.writeSingleField(e.Value, @out);
                    }
                }

                public override string ToString()
                {
                    StringBuilder sb = new StringBuilder("map<");
                    sb.Append(keyType).Append(", ").Append(valueType).Append(">");
                    return sb.ToString();
                }

                public override bool Equals(object obj)
                {
                    if (obj is IMapType)
                    {
                        return keyType.Equals(((IMapType)obj).GetKeyType()) && valueType.Equals(((IMapType)obj).GetValueType());
                    }
                    return false;
                }
            }
        }
    }
}