using System.IO;

namespace de.ust.skill.common.csharp
{
    namespace streams
    {

        /// <summary>
        /// This stream is used to parse a mapped region of field data.
        /// 
        /// @author Simon Glaub, Timm Felden
        /// </summary>
        public class MappedInStream : InStream
        {

            public MappedInStream(BinaryReader input) : base(input)
            {
            }

            /// <summary>
            /// creates a view onto this buffer that will not affect the buffer itself
            /// </summary>
            /// <param name="begin"> relative beginning of the mapped stream </param>
            /// <param name="end"> relative ending of the mapped stream </param>
            /// <returns> a new mapped in stream, that can read from begin to end </returns>
            public virtual MappedInStream view(int begin, int end)
            {
                int pos = (int)input.BaseStream.Position;
                MemoryStream ms = new MemoryStream(pos + end);
                input.BaseStream.CopyTo(ms);
                BinaryReader r = new BinaryReader(ms);
                r.BaseStream.Position = pos + begin;
                return new MappedInStream(r);
            }

            public virtual BinaryReader asByteBuffer()
            {
                return input;
            }

            public override void jump(long position)
            {
                throw new System.InvalidOperationException("there is no sane reason to jump in a mapped stream");
            }

            public override string ToString()
            {
                return string.Format("MappedInStream(0x{0:X} -> 0x{1:X}, next: 0x{2:X})", input.BaseStream.Position, input.BaseStream.Length, input.ReadSByte());
            }
        }
    }
}