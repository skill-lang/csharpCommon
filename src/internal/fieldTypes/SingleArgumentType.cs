using System.Collections;
using System.Collections.Generic;

using de.ust.skill.common.csharp.streams;

namespace de.ust.skill.common.csharp
{
    namespace @internal
    {
        namespace fieldTypes
        {

            /// <summary>
            /// Super class of all container types with one type argument.
            /// 
            /// @author Simon Glaub, Timm Felden
            /// </summary>
            public abstract class SingleArgumentType<T, Base> : CompoundType<T>, ISingleArgumentType where T : IEnumerable
            {

                public readonly FieldType groundType;

                public SingleArgumentType(int typeID, FieldType groundType) : base(typeID)
                {
                    this.groundType = groundType;
                }

                public FieldType GetGroundType()
                {
                    return groundType;
                }

                public override long calculateOffset(ICollection xs)
                {
                    long result = 0L;
                    foreach (T x in xs)
                    {
                        int size = null == x ? 0 : ((ICollection)x).Count;
                        if (0 == size)
                        {
                            result += 1;
                        }
                        else
                        {
                            result += V64.singleV64Offset(size);
                            result += groundType.calculateOffset((ICollection)x);
                        }
                    }

                    return result;
                }

                public override long singleOffset(object x)
                {
                    if (null == x)
                    {
                        return 1L;
                    }

                    return V64.singleV64Offset(((ICollection)x).Count) + groundType.calculateOffset((ICollection)x);
                }

                public override void writeSingleField(object x, OutStream @out)
                {
                    int size = null == x ? 0 : ((ICollection)x).Count;
                    if (0 == size)
                    {
                        @out.i8(0);
                    }
                    else
                    {
                        @out.v64(size);
                        foreach (Base e in (T)x)
                        {
                            groundType.writeSingleField(e, @out);
                        }
                    }
                }
            }
        }
    }
}