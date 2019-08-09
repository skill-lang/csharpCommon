using SkillException = de.ust.skill.common.csharp.api.SkillException;
using de.ust.skill.common.csharp.streams;

namespace de.ust.skill.common.csharp
{
    namespace restrictions
    {

        /// <summary>
        /// Factory and implementations for all range restrictions
        /// 
        /// @author Simon Glaub, Timm Felden
        /// </summary>
        public class Range
        {

            private Range()
            {
                // no instance
            }

            /// <returns> a restriction on success or null on error </returns>
            public static IFieldRestriction make(int typeID, InStream @in)
            {
                switch (typeID)
                {
                    case 7:
                        return new RangeI8(@in.i8(), @in.i8());
                    case 8:
                        return new RangeI16(@in.i16(), @in.i16());
                    case 9:
                        return new RangeI32(@in.i32(), @in.i32());
                    case 10:
                        return new RangeI64(@in.i64(), @in.i64());
                    case 11:
                        return new RangeV64(@in.v64(), @in.v64());
                    case 12:
                        return new RangeF32(@in.f32(), @in.f32());
                    case 13:
                        return new RangeF64(@in.f64(), @in.f64());
                    default:
                        return null;
                }
            }

            sealed class RangeI8 : FieldRestriction<sbyte>
            {

                private readonly sbyte min;
                private readonly sbyte max;

                internal RangeI8(sbyte min, sbyte max)
                {
                    this.min = min;
                    this.max = max;
                }

                public void check(sbyte value)
                {
                    if (value < min || max < value)
                    {
                        throw new SkillException(string.Format("{0} is not in Range({1:D}, {2:D})", value, min, max));
                    }
                }
            }

            sealed class RangeI16 : FieldRestriction<short>
            {

                private readonly short min;
                private readonly short max;

                internal RangeI16(short min, short max)
                {
                    this.min = min;
                    this.max = max;
                }

                public void check(short value)
                {
                    if (value < min || max < value)
                    {
                        throw new SkillException(string.Format("{0} is not in Range({1:D}, {2:D})", value, min, max));
                    }
                }
            }

            sealed class RangeI32 : FieldRestriction<int>
            {

                private readonly int min;
                private readonly int max;

                internal RangeI32(int min, int max)
                {
                    this.min = min;
                    this.max = max;
                }

                public void check(int value)
                {
                    if (value < min || max < value)
                    {
                        throw new SkillException(string.Format("{0} is not in Range({1:D}, {2:D})", value, min, max));
                    }
                }
            }

            sealed class RangeI64 : FieldRestriction<long>
            {

                private readonly long min;
                private readonly long max;

                internal RangeI64(long min, long max)
                {
                    this.min = min;
                    this.max = max;

                }

                public void check(long value)
                {
                    if (value < min || max < value)
                    {
                        throw new SkillException(string.Format("{0} is not in Range({1:D}, {2:D})", value, min, max));
                    }
                }
            }

            sealed class RangeV64 : FieldRestriction<long>
            {

                private readonly long min;
                private readonly long max;

                internal RangeV64(long min, long max)
                {
                    this.min = min;
                    this.max = max;
                }

                public void check(long value)
                {
                    if (value < min || max < value)
                    {
                        throw new SkillException(string.Format("{0} is not in Range({1:D}, {2:D})", value, min, max));
                    }
                }
            }

            sealed class RangeF32 : FieldRestriction<float>
            {

                private readonly float min;
                private readonly float max;

                internal RangeF32(float min, float max)
                {
                    this.min = min;
                    this.max = max;
                }

                public void check(float value)
                {
                    if (value < min || max < value)
                    {
                        throw new SkillException(string.Format("{0} is not in Range({1:D}, {2:D})", value, min, max));
                    }
                }
            }

            sealed class RangeF64 : FieldRestriction<double>
            {

                private readonly double min;
                private readonly double max;

                internal RangeF64(double min, double max)
                {
                    this.min = min;
                    this.max = max;
                }

                public void check(double value)
                {
                    if (value < min || max < value)
                    {
                        throw new SkillException(string.Format("{0} is not in Range({1:D}, {2:D})", value, min, max));
                    }
                }
            }
        }
    }
}