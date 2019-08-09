using de.ust.skill.common.csharp.@internal.parts;
using de.ust.skill.common.csharp.streams;

namespace de.ust.skill.common.csharp
{
    namespace @internal
    {
        namespace fieldDeclarations
        {
            /// <summary>
            /// This trait marks auto fields.
            /// 
            /// @author Simon Glaub, Timm Felden
            /// </summary>
            public abstract class AutoField<T, Obj> : FieldDeclaration<T, Obj>, KnownField<T, Obj>, IAutoField where Obj : SkillObject
            {
                protected AutoField(FieldType type, string name, int index, AbstractStoragePool owner) : base(type, name, index, owner)
                {
                }

                public override void rsc(int i, int h, MappedInStream @in)
                {
                    throw new System.MissingMethodException("one can not read auto fields!");
                }

                public override void rbc(BulkChunk last, MappedInStream @in)
                {
                    throw new System.MissingMethodException("one can not read auto fields!");
                }

                public override void obc(BulkChunk c)
                {
                    throw new System.MissingMethodException("one get the offset of an auto fields!");
                }

                public override void osc(int i, int end)
                {
                    throw new System.MissingMethodException("one get the offset of an auto fields!");
                }

                public override void wbc(BulkChunk c, MappedOutStream @out)
                {
                    throw new System.MissingMethodException("one can not write auto fields!");
                }

                public override void wsc(int i, int end, MappedOutStream @out)
                {
                    throw new System.MissingMethodException("one can not write auto fields!");
                }
            }
        }
    }
}