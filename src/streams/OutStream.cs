using System;
using System.IO;

namespace de.ust.skill.common.csharp
{
    namespace streams
    {

        /// <summary>
        /// Encapsulation of memory mapped files that allows to use skill types and mmap efficiently.
        /// 
        /// @author Simon Glaub, Timm Felden
        /// </summary>
        public abstract class OutStream
        {

            protected BinaryWriter buffer;

            // current write position
            protected long position = 0L;

            public virtual long Position
            {
                get
                {
                    if (null == buffer)
                    {
                        return position;
                    }

                    return buffer.BaseStream.Position + position;
                }
            }

            protected OutStream(BinaryWriter buffer)
            {
                this.buffer = buffer;
            }

            protected abstract void refresh();

            private long Remaining()
            {
                if (buffer.BaseStream is MemoryStream)
                {
                    return ((MemoryStream)buffer.BaseStream).Capacity - buffer.BaseStream.Position;
                }
                else
                {
                    return buffer.BaseStream.Length - buffer.BaseStream.Position;
                }
            }

            public void @bool(bool data)
            {
                if (null == buffer || Remaining() < 1)
                {
                    refresh();
                }
                buffer.Write(data ? unchecked((sbyte)0xFF) : (sbyte)0x00);
            }

            public void i8(sbyte data)
            {
                if (null == buffer || Remaining() < 1)
                {
                    refresh();
                }
                buffer.Write(data);
            }

            public void i16(short data)
            {
                if (null == buffer || Remaining() < 2)
                {
                    refresh();
                }
                byte[] bytes = BitConverter.GetBytes(data);
                Array.Reverse(bytes);
                buffer.Write(bytes);
            }

            public void i32(int data)
            {
                if (null == buffer || Remaining() < 4)
                {
                    refresh();
                }
                byte[] bytes = BitConverter.GetBytes(data);
                Array.Reverse(bytes);
                buffer.Write(bytes);
            }

            public void i64(long data)
            {
                if (null == buffer || Remaining() < 8)
                {
                    refresh();
                }
                byte[] bytes = BitConverter.GetBytes(data);
                Array.Reverse(bytes);
                buffer.Write(bytes);
            }

            public abstract void v64(int data);

            public abstract void v64(long data);

            public void f32(float data)
            {
                if (null == buffer || Remaining() < 4)
                {
                    refresh();
                }
                byte[] bytes = BitConverter.GetBytes(data);
                Array.Reverse(bytes);
                buffer.Write(bytes);
            }

            public void f64(double data)
            {
                if (null == buffer || Remaining() < 8)
                {
                    refresh();
                }
                byte[] bytes = BitConverter.GetBytes(data);
                Array.Reverse(bytes);
                buffer.Write(bytes);
            }

            /// <summary>
            /// close the out stream; this should not be necessary, but apparently, windows does not play by the rules
            /// </summary>
            /// <exception cref="IOException"> </exception>
            public abstract void close();
        }
    }
}