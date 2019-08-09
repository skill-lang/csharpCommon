using System;

using SkillException = de.ust.skill.common.csharp.api.SkillException;
using FieldDeclaration = de.ust.skill.common.csharp.api.FieldDeclaration;

namespace de.ust.skill.common.csharp
{
    namespace @internal
    {
        namespace exceptions
        {

            /// <summary>
            /// Thrown, if field deserialization consumes less bytes then specified by the header.
            /// 
            /// @author Simon Glaub, Timm Felden
            /// </summary>
            public class PoolSizeMissmatchError : SkillException
            {

                public PoolSizeMissmatchError(int block, long position, long begin, long end, FieldDeclaration field) : base(string.Format("Corrupted data chunk in block {0:D} at 0x{1:X} between 0x{2:X} and 0x{3:X} in Field {4}.{5} of type: {6}", block + 1, position, begin, end, field.Owner.Name, field.Name, field.Type.ToString()))
                {
                }

                public PoolSizeMissmatchError(int block, long begin, long end, FieldDeclaration field, InvalidOperationException e) : base(string.Format("Corrupted data chunk in block {0:D} between 0x{1:X} and 0x{2:X} in Field {3}.{4} of type: {5}", block + 1, begin, end, field.Owner.Name, field.Name, field.Type.ToString()), e)
                {
                }

            }
        }
    }
}