using System.Collections;
using System.Collections.Generic;
using de.ust.skill.common.csharp.@internal.fieldDeclarations;
using de.ust.skill.common.csharp.streams;

namespace de.ust.skill.common.csharp
{
    namespace @internal
    {

        /// <summary>
        /// Top level implementation of a field type, the runtime representation of a
        /// fields type.
        /// @note representation of the type system relies on invariants and heavy abuse
        ///       of type erasure
        /// @author Simon Glaub, Timm Felden
        /// </summary>
        public abstract class FieldType : api.FieldType
        {

            readonly int typeID;

            public override int TypeID
            {
                get
                {

                    return typeID;
                }
            }

            protected FieldType(int typeID)
            {
                this.typeID = typeID;
            }

            public override bool Equals(object obj)
            {
                if (obj is FieldType)
                {
                    return ((FieldType)obj).TypeID == typeID;
                }
                return false;
            }

            public override sealed int GetHashCode()
            {
                return typeID;
            }

            /// <summary>
            /// Takes one T out of the stream.
            /// 
            /// @note intended for internal usage only!
            /// </summary>
            public abstract object readSingleField(InStream @in);

            /// <summary>
            /// Calculate the amount of disk space required to store all xs.
            /// </summary>
            public abstract long calculateOffset(ICollection xs);

            /// <summary>
            /// Calculate the amount of disk space required to store x.
            /// </summary>
            public abstract long singleOffset(object x);

            /// <summary>
            /// Puts one T into the stream.
            /// 
            /// @note intended for internal usage only!
            /// </summary>
            public abstract void writeSingleField(object data, OutStream @out);

        }
    }
}