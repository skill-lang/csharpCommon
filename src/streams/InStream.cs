using System;
using System.Diagnostics;
using System.IO;

namespace de.ust.skill.common.csharp
{
    namespace streams
    {

        /// <summary>
        /// Implementations of this class are used to turn a byte stream into a stream of
        /// integers and floats.
        /// 
        /// @author Simon Glaub, Timm Felden
        /// </summary>
        public abstract class InStream
        {
            protected readonly BinaryReader input;

            protected InStream(BinaryReader input)
            {
                this.input = input;
            }

            /// <returns> take an f64 from the stream </returns>
            public double f64()
            {
                byte[] bytes = input.ReadBytes(8);
                Array.Reverse(bytes);
                return BitConverter.ToDouble(bytes, 0);
            }

            /// <returns> take an f32 from the stream </returns>
            public float f32()
            {
                byte[] bytes = input.ReadBytes(4);
                Array.Reverse(bytes);
                return BitConverter.ToSingle(bytes, 0);
            }

            /// <returns> take an v32 from the stream </returns>
            public int v32()
            {
                int rval;

                return (0 != ((rval = i8()) & 0x80)) ? multiByteV32(rval) : rval;
            }

            /// <summary>
            /// multi byte v32 values are treated in a different function to enable
            /// inlining of more common single byte v32 values
            /// </summary>
            private int multiByteV32(int rval)
            {
                int r;
                rval = (rval & 0x7f) | (((r = i8()) & 0x7f) << 7);

                if (0 != (r & 0x80))
                {
                    rval |= ((r = i8()) & 0x7f) << 14;

                    if (0 != (r & 0x80))
                    {
                        rval |= ((r = i8()) & 0x7f) << 21;

                        if (0 != (r & 0x80))
                        {
                            rval |= ((r = i8()) & 0x7f) << 28;

                            if (0 != (r & 0x80))
                            {
                                throw new System.InvalidOperationException("unexpected overlong v64 value (expected 32bit)");
                            }
                        }
                    }
                }
                return rval;
            }

            /// <returns> take an v64 from the stream </returns>
            public long v64()
            {
                long rval;

                return (0 != ((rval = i8()) & 0x80)) ? multiByteV64(rval) : rval;
            }

            /// <summary>
            /// multi byte v64 values are treated in a different function to enable
            /// inlining of more common single byte v64 values
            /// </summary>
            private long multiByteV64(long rval)
            {
                long r;
                rval = (rval & 0x7f) | (((r = i8()) & 0x7f) << 7);

                if (0 != (r & 0x80))
                {
                    rval |= ((r = i8()) & 0x7f) << 14;

                    if (0 != (r & 0x80))
                    {
                        rval |= ((r = i8()) & 0x7f) << 21;

                        if (0 != (r & 0x80))
                        {
                            rval |= ((r = i8()) & 0x7f) << 28;

                            if (0 != (r & 0x80))
                            {
                                rval |= ((r = i8()) & 0x7f) << 35;

                                if (0 != (r & 0x80))
                                {
                                    rval |= ((r = i8()) & 0x7f) << 42;

                                    if (0 != (r & 0x80))
                                    {
                                        rval |= ((r = i8()) & 0x7f) << 49;

                                        if (0 != (r & 0x80))
                                        {
                                            rval |= (((long)i8()) << 56);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                return rval;
            }

            /// <returns> take an i64 from the stream </returns>
            public long i64()
            {
                byte[] bytes = input.ReadBytes(8);
                Array.Reverse(bytes);
                return BitConverter.ToInt64(bytes, 0);
            }

            /// <returns> take an i32 from the stream </returns>
            /// <exception cref="UnexpectedEOF"> if there is no i32 in the stream </exception>
            public int i32()
            {
                byte[] bytes = input.ReadBytes(4);
                Array.Reverse(bytes);
                return BitConverter.ToInt32(bytes, 0);
            }

            /// <returns> take an i16 from the stream </returns>
            public short i16()
            {
                byte[] bytes = input.ReadBytes(2);
                Array.Reverse(bytes);
                return BitConverter.ToInt16(bytes, 0);
            }

            /// <returns> take an i8 from the stream </returns>
            public sbyte i8()
            {
                return input.ReadSByte();
            }

            /// <returns> take a bool from the stream </returns>
            public bool @bool()
            {
                return input.ReadBoolean();
            }

            /// <returns> raw byte array taken from the stream </returns>
            public byte[] bytes(int length)
            {
                return input.ReadBytes(length);
            }

            /// <returns> true iff there are at least n bytes left in the stream </returns>
            public bool has(int n)
            {
                return input.BaseStream.Length >= n + input.BaseStream.Position;
            }

            /// <returns> true iff at the end of file (or stream) </returns>
            public bool eof()
            {
                return input.BaseStream.Position == input.BaseStream.Length;
            }

            /// <summary>
            /// use with care!
            /// </summary>
            /// <param name="position"> jump to target position, without the ability to restore the
            ///            old position </param>
            public abstract void jump(long position);

            public long position()
            {
                return input.BaseStream.Position;
            }
        }
    }
}