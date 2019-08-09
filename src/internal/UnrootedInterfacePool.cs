using System.Collections;
using System.Collections.Generic;

using de.ust.skill.common.csharp.streams;
using de.ust.skill.common.csharp.@internal.fieldTypes;
using de.ust.skill.common.csharp.api;

namespace de.ust.skill.common.csharp
{
    namespace @internal
    {

        /// <summary>
        /// Holds interface instances. Serves as an API realization. Ensures correctness
        /// of reflective type system.
        /// 
        /// @note in this case, the super type is annotation
        /// @note unfortunately, one cannot prove that T extends SkillObject. Hence, we
        ///       cannot inherit from Access<T>
        /// 
        /// @author Simon Glaub, Timm Felden
        /// </summary>
        public sealed class UnrootedInterfacePool<T> : FieldType, api.GeneralAccess<T>, IUnrootedInterfacePool
        {

            private readonly string name;
            private readonly Annotation superType;

            private readonly AbstractStoragePool[] realizations;

            /// <summary>
            /// Construct an interface pool.
            /// 
            /// @note realizations must be in type order
            /// @note realizations must be of type StoragePool<? extends T, ?>
            /// </summary>
            public UnrootedInterfacePool(string name, Annotation superPool, params AbstractStoragePool[] realizations) : base(superPool.TypeID)
            {
                this.name = name;
                superType = superPool;
                this.realizations = realizations;
            }

            public int size()
            {
                int rval = 0;
                foreach (StoragePool<SkillObject, SkillObject> p in realizations)
                {
                    rval += p.size();
                }
                return rval;
            }

            public IEnumerator GetEnumerator()
            {
                return (new InterfaceIterator<T>(realizations)).GetEnumerator();
            }

            public string Name
            {
                get
                {
                    return name;
                }
            }

            public override object readSingleField(InStream @in)
            {
                return (T)superType.readSingleField(@in);
            }

            public override long calculateOffset(ICollection xs)
            {
                return superType.calculateOffset((ICollection) cast<ICollection<T>, ICollection<SkillObject>>((ICollection<T>) xs));
            }

            public override long singleOffset(object x)
            {
                return superType.singleOffset((SkillObject)(object)x);
            }

            public override void writeSingleField(object data, OutStream @out)
            {
                superType.writeSingleField((SkillObject)data, @out);
            }

            /// <summary>
            /// hide cast that is required because interfaces do not inherit from classes
            /// </summary>
            private static U cast<V, U>(V x)
            {
                return (U)(object)x;
            }

            public Annotation Type
            {
                get
                {
                    return superType;
                }
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