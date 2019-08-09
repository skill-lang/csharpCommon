using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using de.ust.skill.common.csharp.streams;

namespace de.ust.skill.common.csharp
{
    namespace @internal
    {
        namespace fieldTypes
        {
            /// <summary>
            /// Annotation types are instantiated once per state.
            /// 
            /// @author Simon Glaub, Timm Felden
            /// </summary>
            public sealed class Annotation : FieldType, ReferenceType
            {

                private readonly List<AbstractStoragePool> types;
                private Dictionary<string, AbstractStoragePool> typeByName = null;

                /// <param name="types"> the array list containing all types valid inside of a state
                /// @note types can grow after passing the pointer to the annotation type. This behavior is required in order to
                ///       implement reflective annotation parsing correctly.
                /// @note can not take a state as argument, because it may not exist yet </param>
                public Annotation(List<AbstractStoragePool> types) : base(5)
                {
                    this.types = types;
                    Debug.Assert(types != null);
                }

                public void fixTypes(Dictionary<string, AbstractStoragePool> poolByName)
                {
                    Debug.Assert(typeByName == null);
                    typeByName = poolByName;
                }

                public override object readSingleField(InStream @in)
                {
                    int t = @in.v32();
                    int f = @in.v32();
                    if (0 == t)
                    {
                        return null;
                    }
                    return (SkillObject)types[t - 1].getByID(f);
                }

                public override long calculateOffset(ICollection xs)
                {
                    long result = 0L;
                    foreach (SkillObject @ref in xs)
                    {
                        if (null == @ref)
                        {
                            result += 2;
                        }
                        else
                        {
                            if (@ref is NamedType)
                            {
                                result += V64.singleV64Offset(((NamedType)@ref).ΤPool.TypeID - 31);
                            }
                            else
                            {
                                result += V64.singleV64Offset(typeByName[@ref.skillName()].TypeID - 31);
                            }

                            result += V64.singleV64Offset(@ref.SkillID);
                        }
                    }

                    return result;
                }

                /// <summary>
                /// used for simple offset calculation
                /// </summary>
                public override long singleOffset(object @ref)
                {
                    if (null == @ref)
                    {
                        return 2L;
                    }

                    long name;
                    if (@ref is NamedType)
                    {
                        name = V64.singleV64Offset(((NamedType)@ref).ΤPool.TypeID - 31);
                    }
                    else
                    {
                        name = V64.singleV64Offset(typeByName[((SkillObject)@ref).skillName()].TypeID - 31);
                    }

                    return name + V64.singleV64Offset(((SkillObject)@ref).SkillID);
                }

                public override void writeSingleField(object @ref, OutStream @out)
                {
                    if (null == @ref)
                    {
                        // magic trick!
                        @out.i16((short)0);
                        return;
                    }

                    if (@ref is NamedType)
                    {
                        @out.v64(((NamedType)@ref).ΤPool.TypeID - 31);
                    }
                    else
                    {
                        @out.v64(typeByName[((SkillObject)@ref).skillName()].TypeID - 31);
                    }
                    @out.v64(((SkillObject)@ref).SkillID);
                }

                public override string ToString()
                {
                    return "annotation";
                }

                /// <summary>
                /// required for proper treatment of Interface types
                /// </summary>
                public static Annotation cast(FieldType f)
                {
                    return (Annotation)f;
                }

                public override api.FieldType cast<K, V>()
                {
                    throw new System.NotImplementedException();
                }
            }
        }
    }
}