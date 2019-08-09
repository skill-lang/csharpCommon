using System;

using SkillException = de.ust.skill.common.csharp.api.SkillException;

namespace de.ust.skill.common.csharp
{
    namespace @internal
    {
        namespace exceptions
        {

            /// <summary>
            /// Thrown, if an index into a pool is invalid.
            /// 
            /// @author Simon Glaub, Timm Felden
            /// </summary>
            public class InvalidPoolIndexException : SkillException
            {

                public InvalidPoolIndexException(long index, int size, string pool) : base(string.Format("Invalid index {0:D} into pool {1} of size {2:D}", index, pool, size))
                {
                }

                public InvalidPoolIndexException(long index, int size, string pool, Exception cause) : base(string.Format("Invalid index {0:D} into pool {1} of size {2:D}", index, pool, size), cause)
                {
                }
            }
        }
    }
}