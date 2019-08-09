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

            public sealed class V64 : IntegerType<long>
            {

                private static readonly V64 instance = new V64();

                public static V64 get()
                {
                    return instance;
                }

                private V64() : base(11)
                {
                }

                public override object readSingleField(InStream @in)
                {
                    return @in.v64();
                }

                public override long calculateOffset(ICollection xs)
                {
                    long result = 0L;
                    foreach (long v in xs)
                    {
                        if (0L == ((ulong)v & 0xFFFFFFFFFFFFFF80L))
                        {
                            result += 1;
                        }
                        else if (0L == ((ulong)v & 0xFFFFFFFFFFFFC000L))
                        {
                            result += 2;
                        }
                        else if (0L == ((ulong)v & 0xFFFFFFFFFFE00000L))
                        {
                            result += 3;
                        }
                        else if (0L == ((ulong)v & 0xFFFFFFFFF0000000L))
                        {
                            result += 4;
                        }
                        else if (0L == ((ulong)v & 0xFFFFFFF800000000L))
                        {
                            result += 5;
                        }
                        else if (0L == ((ulong)v & 0xFFFFFC0000000000L))
                        {
                            result += 6;
                        }
                        else if (0L == ((ulong)v & 0xFFFE000000000000L))
                        {
                            result += 7;
                        }
                        else if (0L == ((ulong)v & 0xFF00000000000000L))
                        {
                            result += 8;
                        }
                        else
                        {
                            result += 9;
                        }
                    }
                    return result;
                }

                public override long singleOffset(object @ref)
                {
                    object v = null == @ref ? 0 : @ref;
                    if (0L == ((ulong)v & 0xFFFFFFFFFFFFFF80L))
                    {
                        return 1;
                    }
                    else if (0L == ((ulong)v & 0xFFFFFFFFFFFFC000L))
                    {
                        return 2;
                    }
                    else if (0L == ((ulong)v & 0xFFFFFFFFFFE00000L))
                    {
                        return 3;
                    }
                    else if (0L == ((ulong)v & 0xFFFFFFFFF0000000L))
                    {
                        return 4;
                    }
                    else if (0L == ((ulong)v & 0xFFFFFFF800000000L))
                    {
                        return 5;
                    }
                    else if (0L == ((ulong)v & 0xFFFFFC0000000000L))
                    {
                        return 6;
                    }
                    else if (0L == ((ulong)v & 0xFFFE000000000000L))
                    {
                        return 7;
                    }
                    else if (0L == ((ulong)v & 0xFF00000000000000L))
                    {
                        return 8;
                    }
                    else
                    {
                        return 9;
                    }
                }

                public override void writeSingleField(object target, OutStream @out)
                {
                    @out.v64((long)target);
                }

                public override string ToString()
                {
                    return "v64";
                }

                /// <summary>
                /// helper method used by other offset calculations
                /// </summary>
                public static long singleV64Offset(int v)
                {
                    if (0 == (v & 0xFFFFFF80))
                    {
                        return 1;
                    }
                    else if (0 == (v & 0xFFFFC000))
                    {
                        return 2;
                    }
                    else if (0 == (v & 0xFFE00000))
                    {
                        return 3;
                    }
                    else if (0 == (v & 0xF0000000))
                    {
                        return 4;
                    }
                    else
                    {
                        return 5;
                    }
                }

                /// <summary>
                /// helper method used by other offset calculations
                /// </summary>
                public static long singleV64Offset(long v)
                {
                    if (0L == ((ulong)v & 0xFFFFFFFFFFFFFF80L))
                    {
                        return 1;
                    }
                    else if (0L == ((ulong)v & 0xFFFFFFFFFFFFC000L))
                    {
                        return 2;
                    }
                    else if (0L == ((ulong)v & 0xFFFFFFFFFFE00000L))
                    {
                        return 3;
                    }
                    else if (0L == ((ulong)v & 0xFFFFFFFFF0000000L))
                    {
                        return 4;
                    }
                    else if (0L == ((ulong)v & 0xFFFFFFF800000000L))
                    {
                        return 5;
                    }
                    else if (0L == ((ulong)v & 0xFFFFFC0000000000L))
                    {
                        return 6;
                    }
                    else if (0L == ((ulong)v & 0xFFFE000000000000L))
                    {
                        return 7;
                    }
                    else if (0L == ((ulong)v & 0xFF00000000000000L))
                    {
                        return 8;
                    }
                    else
                    {
                        return 9;
                    }
                }

                public override api.FieldType cast<K, V>()
                {
                    throw new System.NotImplementedException();
                }
            }
        }
    }
}