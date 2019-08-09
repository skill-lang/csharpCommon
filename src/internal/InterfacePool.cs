using System.Collections;
using System.Collections.Generic;

using SkillFile = de.ust.skill.common.csharp.api.SkillFile;
using de.ust.skill.common.csharp.@internal.fieldTypes;
using de.ust.skill.common.csharp.streams;
using de.ust.skill.common.csharp.api;

namespace de.ust.skill.common.csharp
{
    namespace @internal
    {

        /// <summary>
        /// Holds interface instances. Serves as an API realization. Ensures correctness
        /// of reflective type system.
        /// 
        /// @note unfortunately, one cannot prove that T extends SkillObject. Hence, we
        ///       cannot inherit from Access<T>
        /// 
        /// 
        /// @author Simon Glaub, Timm Felden
        /// </summary>
        public sealed class InterfacePool<T, B> : FieldType, api.GeneralAccess<T>, IInterfacePool where B : SkillObject
        {

            private readonly string name;
            public readonly AbstractStoragePool superPool;
            private readonly AbstractStoragePool[] realizations;

            /// <summary>
            /// Construct an interface pool.
            /// 
            /// @note realizations must be in type order
            /// </summary>
            public InterfacePool(string name, AbstractStoragePool superPool, params AbstractStoragePool[] realizations) : base(superPool.TypeID)
            {
                this.name = name;
                this.superPool = superPool;
                this.realizations = realizations;
            }

            public int size()
            {
                int rval = 0;
                foreach (AbstractStoragePool p in realizations)
                {
                    rval += p.size();
                }
                return rval;
            }

            public IEnumerator GetEnumerator()
            {
                return (new InterfaceIterator<T>(realizations)).GetEnumerator();
            }

            public SkillFile Owner
            {
                get
                {
                    return superPool.Owner;
                }
            }

            public string Name
            {
                get
                {
                    return name;
                }
            }

            public string superName()
            {
                return superPool.Name;
            }

            public override object readSingleField(InStream @in)
            {
                int index = @in.v32() - 1;
                object[] data = superPool.data;
                if (index < 0 | data.Length <= index)
                {
                    return null;
                }
                return (T)data[index];
            }

            public override long calculateOffset(ICollection xs)
            {
                // shortcut small compressed types
                if (superPool.data.Length < 128)
                {
                    return xs.Count;
                }

                long result = 0L;
                foreach (object x in xs)
                {
                    result += null == x ? 1 : V64.singleV64Offset(((SkillObject)x).skillID);
                }
                return result;
            }

            public override long singleOffset(object x)
            {
                return null == x ? 1 : V64.singleV64Offset(((SkillObject)x).skillID);
            }

            public override void writeSingleField(object data, OutStream @out)
            {
                if (null == data)
                {
                    @out.i8(0);
                }
                else
                {
                    @out.v64(((SkillObject)data).skillID);
                }
            }

            public AbstractStoragePool getSuperPool()
            {
                return superPool;
            }

            public override string ToString()
            {
                return name;
            }

            public override api.FieldType cast<K, V>()
            {
                throw new System.NotImplementedException();
            }
        }
    }
}