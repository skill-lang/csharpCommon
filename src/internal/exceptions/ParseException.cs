using System;

using SkillException = de.ust.skill.common.csharp.api.SkillException;
using InStream = de.ust.skill.common.csharp.streams.InStream;

namespace de.ust.skill.common.csharp
{
    namespace @internal
    {
        namespace exceptions
        {

            /// <summary>
            /// This exception is used if byte stream related errors occur.
            /// 
            /// @author Simon Glaub, Timm Felden
            /// </summary>
            public sealed class ParseException : SkillException
            {
                public ParseException(SkillException cause, string msg) : base(msg, cause)
                {
                }

                public ParseException(InStream @in, int block, Exception cause, string msg) : base(string.Format("In block {0:D} @0x{1:x}: {2}", block + 1, @in.position(), msg), cause)
                {
                }

                public ParseException(InStream @in, int block, Exception cause, string msgFormat, params object[] msgArgs) : base(string.Format("In block {0:D} @0x{1:x}: {2}", block + 1, @in.position(), string.Format(msgFormat, msgArgs)), cause)
                {
                }
            }
        }
    }
}