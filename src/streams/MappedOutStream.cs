using System.Collections.Generic;
using System.IO;

namespace de.ust.skill.common.csharp
{
    namespace streams
    {

        /// <summary>
        /// Allows writing to memory mapped region.
        /// 
        /// @author Simon Glaub, Timm Felden
        /// </summary>
        public sealed class MappedOutStream : OutStream
        {
            private readonly FileStream file;

            private List<MappedOutStream> clonedMaps = new List<MappedOutStream>();

            internal MappedOutStream(BinaryWriter buffer, FileStream file, long position) : base(buffer)
            {
                this.file = file;
                this.position = position;
            }

            /// <returns> reveals the internal buffer </returns>
            public BinaryWriter getBuffer()
            {
                return buffer;
            }

            /// <summary>
            /// creates a copy of this in the argument range
            /// </summary>
            public MappedOutStream clone(int begin, int end)
            {
                MemoryStream ms = new MemoryStream(end - begin);
                BinaryWriter b = new BinaryWriter(ms);
                MappedOutStream clonedMap = new MappedOutStream(b, file, begin);
                clonedMaps.Add(clonedMap);
                return clonedMap;
            }

            protected override void refresh()
            {
                // do nothing; let the JIT remove this method and all related checks
            }

            public override void v64(int v)
            {
                if (0 == (v & 0xFFFFFF80))
                {
                    buffer.Write((sbyte)v);
                }
                else
                {
                    buffer.Write(unchecked((sbyte)(0x80 | v)));
                    if (0 == (v & 0xFFFFC000))
                    {
                        buffer.Write((sbyte)(v >> 7));
                    }
                    else
                    {
                        buffer.Write(unchecked((sbyte)(0x80 | v >> 7)));
                        if (0 == (v & 0xFFE00000))
                        {
                            buffer.Write((sbyte)(v >> 14));
                        }
                        else
                        {
                            buffer.Write(unchecked((sbyte)(0x80 | v >> 14)));
                            if (0 == (v & 0xF0000000))
                            {
                                buffer.Write((sbyte)(v >> 21));
                            }
                            else
                            {
                                buffer.Write(unchecked((sbyte)(0x80 | v >> 21)));
                                buffer.Write((sbyte)(v >> 28));
                            }
                        }
                    }
                }
            }

            public override void v64(long v)
            {
                if (0L == ((ulong)v & 0xFFFFFFFFFFFFFF80L))
                {
                    buffer.Write((sbyte)v);
                }
                else
                {
                    v64Medium(v);
                }
            }

            private void v64Medium(long v)
            {
                buffer.Write(unchecked((sbyte)(0x80L | v)));
                if (0L == ((ulong)v & 0xFFFFFFFFFFFFC000L))
                {
                    buffer.Write((sbyte)(v >> 7));
                }
                else
                {
                    v64Large(v);
                }
            }

            private void v64Large(long v)
            {
                buffer.Write(unchecked((sbyte)(0x80L | v >> 7)));
                if (0L == ((ulong)v & 0xFFFFFFFFFFE00000L))
                {
                    buffer.Write((sbyte)(v >> 14));
                }
                else
                {
                    buffer.Write(unchecked((sbyte)(0x80L | v >> 14)));
                    if (0L == ((ulong)v & 0xFFFFFFFFF0000000L))
                    {
                        buffer.Write((sbyte)(v >> 21));
                    }
                    else
                    {
                        buffer.Write(unchecked((sbyte)(0x80L | v >> 21)));
                        if (0L == ((ulong)v & 0xFFFFFFF800000000L))
                        {
                            buffer.Write((sbyte)(v >> 28));
                        }
                        else
                        {
                            buffer.Write(unchecked((sbyte)(0x80L | v >> 28)));
                            if (0L == ((ulong)v & 0xFFFFFC0000000000L))
                            {
                                buffer.Write((sbyte)(v >> 35));
                            }
                            else
                            {
                                buffer.Write(unchecked((sbyte)(0x80L | v >> 35)));
                                if (0L == ((ulong)v & 0xFFFE000000000000L))
                                {
                                    buffer.Write((sbyte)(v >> 42));
                                }
                                else
                                {
                                    buffer.Write(unchecked((sbyte)(0x80L | v >> 42)));
                                    if (0L == ((ulong)v & 0xFF00000000000000L))
                                    {
                                        buffer.Write((sbyte)(v >> 49));
                                    }
                                    else
                                    {
                                        buffer.Write(unchecked((sbyte)(0x80L | v >> 49)));
                                        buffer.Write((sbyte)(v >> 56));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            private void flush()
            {
                if (null != buffer && clonedMaps.Count >= 0)
                {
                    //int p = (int)buffer.BaseStream.Position;
                    //((MemoryStream)buffer.BaseStream).Capacity = p;
                    //byte[] bytes = ((MemoryStream)buffer.BaseStream).ToArray();
                    //file.Write(bytes, 0, bytes.Length);
                    long tmpPosition = file.Position;
                    foreach (MappedOutStream map in clonedMaps)
                    {  
                        file.Position = position + map.position;
                        byte[] bytes = ((MemoryStream)map.buffer.BaseStream).ToArray();
                        file.Write(bytes, 0, bytes.Length);
                    }
                    file.Position = tmpPosition;
                }
            }

            public override void close()
            {
                flush();
                buffer = null;
            }
        }
    }
}