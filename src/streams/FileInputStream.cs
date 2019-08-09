using System.IO;
using System.Runtime.CompilerServices;

namespace de.ust.skill.common.csharp
{
    namespace streams
    {

        /// <summary>
        /// FileChannel based input stream.
        /// 
        /// @author Simon Glaub, Timm Felden
        /// </summary>
        public sealed class FileInputStream : InStream
        {

            private int storedPosition;
            private readonly string path;
            private readonly FileStream file;
            /// <summary>
            /// true iff the file is shared with an output channel
            /// </summary>
            private bool sharedFile = false;

            internal FileStream File
            {
                get
                {
                    sharedFile = true;
                    return file;
                }
            }

            public static FileInputStream open(string path, bool readOnly)
            {
                FileStream file = readOnly ? new FileStream(path, FileMode.OpenOrCreate, FileAccess.Read) : new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                return new FileInputStream(file, path, readOnly);
            }

            private FileInputStream(FileStream file, string path, bool readOnly) : base(new BinaryReader(file))
            {
                this.file = file;
                this.path = path;
            }

            /// <summary>
            /// Maps a part of a file not changing the position of the file stream.
            /// </summary>
            /// <param name="basePosition">
            ///            absolute start index of the mapped region </param>
            /// <param name="begin">
            ///            begin offset of the mapped region </param>
            /// <param name="end">
            ///            end offset of the mapped region </param>
            [MethodImpl(MethodImplOptions.Synchronized)]
            public MappedInStream map(long basePosition, long begin, long end)
            {
                MemoryStream ms = new MemoryStream((int)(end - begin));
                long tmpPosition = input.BaseStream.Position;
                input.BaseStream.Position = basePosition + begin;
                input.BaseStream.CopyTo(ms);
                input.BaseStream.Position = tmpPosition;
                ms.Position = (0);
                ms.SetLength((int)(end-begin));
                BinaryReader r = new BinaryReader(ms);
                return new MappedInStream(r);
            }

            public override void jump(long position)
            {
                input.BaseStream.Position = (int)position;
            }

            /// <summary>
            /// Maps from current position until offset.
            /// </summary>
            /// <returns> a buffer that has exactly offset many bytes remaining </returns>
            public MappedInStream jumpAndMap(int offset)
            {
                BinaryReader r = new BinaryReader(input.BaseStream);
                long pos = input.BaseStream.Position;
                r.BaseStream.SetLength(pos + offset);
                input.BaseStream.Position = pos + offset;
                return new MappedInStream(r);
            }

            /// <summary>
            /// save current position and jump to a new one
            /// </summary>
            public void push(long position)
            {
                storedPosition = (int)input.BaseStream.Position;
                input.BaseStream.Position = (int)position;
            }

            /// <summary>
            /// restore saved position
            /// </summary>
            public void pop()
            {
                input.BaseStream.Position = storedPosition;
            }

            public string Path
            {
                get
                {
                    return path;
                }
            }

            public void close()
            {
                file.Close();
            }
        }
    }
}
