using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using de.ust.skill.common.csharp.api;
using de.ust.skill.common.csharp.streams;

namespace de.ust.skill.common.csharp
{
    namespace @internal
    {
        namespace fieldTypes
        {

            /// <summary>
            /// String types are instantiated once per state.
            /// 
            /// @author Simon Glaub, Timm Felden
            /// </summary>
            public sealed class StringType : FieldType, ReferenceType
            {

                private readonly StringPool strings;
                Dictionary<string, int> stringIDs = null;

                public StringType(StringPool strings) : base(14)
                {
                    this.strings = strings;
                    Debug.Assert(strings != null);
                }

                public override object readSingleField(InStream @in)
                {
                    return strings.get(@in.v32());
                }

                public override long calculateOffset(ICollection xs)
                {
                    // shortcut for small string pools
                    if (stringIDs.Count < 128)
                    {
                        return xs.Count;
                    }

                    long result = 0L;
                    foreach (string s in xs)
                    {
                        result += null == s ? 1 : V64.singleV64Offset(stringIDs[s]);
                    }

                    return result;
                }

                public override long singleOffset(object name)
                {
                    return null == name ? 1 : V64.singleV64Offset(stringIDs[(string)name]);
                }

                public override void writeSingleField(object v, OutStream @out)
                {
                    if (null == v)
                    {
                        @out.i8(0);
                    }
                    else
                    {
                        @out.v64(stringIDs[(string)v]);
                    }

                }

                /// <summary>
                /// internal use only!
                /// 
                /// @note invoked at begin of serialization
                /// </summary>
                public Dictionary<string, int> resetIDs()
                {
                    stringIDs = new Dictionary<string, int>();
                    return stringIDs;
                }

                /// <summary>
                /// internal use only!
                /// 
                /// @note invoked at end of serialization
                /// </summary>
                public void clearIDs()
                {
                    stringIDs = null;
                }

                public override string ToString()
                {
                    return "string";
                }

                public override api.FieldType cast<K, V>()
                {
                    throw new System.NotImplementedException();
                }
            }
        }
    }
}