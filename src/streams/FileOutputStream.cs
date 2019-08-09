using System;
using System.IO;

namespace de.ust.skill.common.csharp
{
    namespace streams
    {

        /// <summary>
        /// BufferedOutputStream based output stream.
        /// 
        /// @author Simon Glaub, Timm Felden
        /// </summary>
        public sealed class FileOutputStream : OutStream
        {

            private const int BUFFERSIZE = 1024;

            private readonly FileStream file;

            private FileOutputStream(FileStream file) : base(new BinaryWriter(new MemoryStream(BUFFERSIZE)))
            {
                // the size is smaller then 4KiB, because headers are expected to be
                // 1KiB at most
                this.file = file;
            }

            /// <summary>
            /// workaround for a bug involving read_write maps and read + append
            /// FileChannels
            /// </summary>
            private FileOutputStream(FileStream file, long position) : base(new BinaryWriter(new MemoryStream(BUFFERSIZE)))
            {
                // the size is smaller then 4KiB, because headers are expected to be
                // 1KiB at most
                this.file = file;
                this.position = position;
            }

            public static FileOutputStream write(FileInputStream target)
            {
                FileStream f = target.File;
                // can happen after multiple flush operations
                if (f == null)
                {
                    f = new FileStream(target.Path, FileMode.Create, FileAccess.ReadWrite);
                }
                f.Position = 0;
                return new FileOutputStream(f);
            }

            /// <returns> a new file output stream, that is setup to append to the target
            ///         fileoutput stream, that is setup to write the target file </returns>
            /// <exception cref="IOException"> propagated error </exception>
            public static FileOutputStream append(FileInputStream target)
            {
                FileStream fc = target.File;
                // can happen after multiple flushs
                if (fc == null)
                {
                    fc = new FileStream(target.Path, FileMode.Create, FileAccess.ReadWrite);
                }
                // workaround for a bug involving read_write maps and read + append
                // FileChannels
                long size = fc.Length;
                fc.Position = size;
                return new FileOutputStream(fc, size);
            }

            protected override void refresh()
            {
                if (null == buffer)
                {
                    buffer = new BinaryWriter(new MemoryStream(BUFFERSIZE));
                }
                else if (0 != buffer.BaseStream.Position)
                {
                    flush();
                    buffer = new BinaryWriter(new MemoryStream(BUFFERSIZE));
                }
            }

            private void flush()
            {
                if (null != buffer)
                {
                    int p = (int)buffer.BaseStream.Position;
                    ((MemoryStream)buffer.BaseStream).Capacity = p;
                    buffer.BaseStream.Position = 0;
                    long tempPosition = file.Position;
                    byte[] bytes = ((MemoryStream)buffer.BaseStream).ToArray();
                    file.Position = position;
                    file.Write(bytes, 0, bytes.Length);
                    file.Position = tempPosition;
                    position += p;
                }
            }

            /// <summary>
            /// put an array of bytes into the stream
            /// 
            /// @note you may not reuse data after putting it to a stream, because the
            ///       actual put might be a deferred operation 
            /// </summary>
            /// <param name="data">  the data to be written </param>
            public void put(byte[] data)
            {
                if (data.Length > BUFFERSIZE)
                {
                    if (null != buffer)
                    {
                        flush();
                        buffer = null;
                    }
                    long tempPosition = file.Position;
                    file.Position = position;
                    file.Write(data, 0, data.Length);
                    file.Position = tempPosition;
                    position += data.Length;
                }
                else
                {
                    if (null == buffer || (((MemoryStream)buffer.BaseStream).Capacity - buffer.BaseStream.Position) < data.Length)
                    {
                        refresh();
                    }
                    buffer.Write(data);
                }
            }

            /// <summary>
            /// Create a map and advance position by the map's size
            /// </summary>
            /// <param name="size"> size of the mapped region </param>
            /// <returns> the created map </returns>
            /// <exception cref="IOException"> </exception>
            public MappedOutStream mapBlock(int size)
            {
                if (null != buffer)
                {
                    flush();
                    buffer = null;
                }
                long pos = Position;
                MemoryStream mappedFile = new MemoryStream(size);
                BinaryWriter r = new BinaryWriter(mappedFile);
                position = pos + size;
                MappedOutStream map = new MappedOutStream(r, file, pos);
                return map;
            }

            /// <summary>
            /// put a ByteBuffer into the stream
            /// 
            /// @note you may not reuse data after putting it to a stream, because the
            ///       actual put might be a deferred operation 
            /// </summary>
            /// <param name="data"> the data to be written </param>
            public void put(BinaryWriter data)
            {
                if (null != buffer)
                {
                    flush();
                    buffer = null;
                }
                int size = (int)(((MemoryStream)data.BaseStream).Capacity - data.BaseStream.Position);
                long tempPosition = file.Position;
                file.Position = position;
                byte[] bytes = ((MemoryStream)data.BaseStream).ToArray();
                file.Write(bytes, 0 ,bytes.Length);
                file.Position = tempPosition;
                position += size;
            }

            /// <summary>
            /// signal the stream to close
            /// </summary>
            public override void close()
            {
                flush();
                if (file.Length > position)
                {
                    file.SetLength(position);
                }
                file.Flush(false);
                file.Close();
            }

            public override void v64(int v)
            {
                if (null == buffer || (((MemoryStream)buffer.BaseStream).Capacity - buffer.BaseStream.Position) < 5)
                {
                    refresh();
                }
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
                if (null == buffer || (((MemoryStream)buffer.BaseStream).Capacity - buffer.BaseStream.Position) < 9)
                {
                    refresh();
                }
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
        }
    }
}