using System.IO;
using System.Collections;
using System.Collections.Generic;

using de.ust.skill.common.csharp.api; 
using de.ust.skill.common.csharp.@internal.fieldDeclarations;
using de.ust.skill.common.csharp.@internal.fieldTypes;
using de.ust.skill.common.csharp.@internal.parts;

namespace de.ust.skill.common.csharp
{
    namespace @internal
    {

        /// <summary>
        /// An interface to make it possible to use StoragePool with an unknown type T and B
        /// 
        /// @author Simon Glaub, Timm Felden 
        /// </summary>

        public abstract class AbstractStoragePool : FieldType, IAccess, ReferenceType
        {

            /// <summary>
            /// Builder for new instances of the pool.
            /// 
            /// @author Simon Glaub, Timm Felden
            /// @todo revisit implementation after the pool is completely implemented.
            ///       Having an instance as constructor argument is questionable.
            /// </summary>
            public abstract class Builder<T> where T : SkillObject
            {
                protected AbstractStoragePool pool;
                protected T instance;

                protected Builder(AbstractStoragePool pool, T instance)
                {
                    this.pool = pool;
                    this.instance = instance;
                }

                /// <summary>
                /// registers the object and invalidates the builder
                /// 
                /// </summary>
                /// </summary>
                /// <returns> the created object </returns>
                public abstract T make();
            }

            internal readonly string name;

            // type hierarchy
            public readonly AbstractStoragePool superPool;
            public readonly int typeHierarchyHeight;

            public readonly IBasePool basePool;

            internal AbstractStoragePool nextPool;

            /// <summary>
            /// get: next pool of this hierarchy in weak type order
            /// set: solves type equation
            /// </summary>
            internal AbstractStoragePool NextPool
            {
                get
                {
                    return nextPool;
                }
            
                set
                {
                    nextPool = value;
                }
            }

            /// <summary>
            /// initialize the next pointer
            /// 
            /// @note invoked from base pool
            /// @note destroys subPools, because they are no longer needed
            /// </summary>
            internal static void establishNextPools(List<AbstractStoragePool> types)
            {
                AbstractStoragePool[] L = new AbstractStoragePool[types.Count];

                // walk in reverse and store last reference in L[base]
                for (int i = types.Count - 1; i >= 0; i--)
                {
                    AbstractStoragePool t = types[i];

                    // skip base pools, because their next link has been established by
                    // their sub pools already
                    AbstractStoragePool p = t.superPool;
                    if (null == p)
                    {
                        continue;
                    }

                    // ensure that every pool has a last pointer
                    int id = t.TypeID - 32;
                    if (null == L[id])
                    {
                        L[id] = t;
                    }

                    // insert into parent link
                    if (null == p.nextPool)
                    {
                        L[p.TypeID - 32] = L[id];
                    }
                    else
                    {
                        L[id].NextPool = p.nextPool;
                    }
                    p.NextPool = t;
                }
            }

            /// <summary>
            /// pointer to base-pool-managed data array
            /// </summary>
            public object[] data;

            /// <summary>
            /// names of known fields, the actual field information is given in the
            /// generated addKnownFiled method.
            /// </summary>
            public readonly string[] knownFields;
            public static readonly string[] noKnownFields = new string[0];

            /// <summary>
            /// all fields that are declared as auto, including skillID
            /// 
            /// @note stores fields at index "-f.index"
            /// @note sub-constructor adds auto fields from super types to this array;
            ///       this is an optimization to make iteration O(1); the array cannot
            ///       change anyway
            /// @note the initial type constructor will already allocate an array of the
            ///       correct size, because the right size is statically known (a
            ///       generation time constant)
            /// </summary>
            internal readonly IAutoField[] autoFields;
            /// <summary>
            /// used as placeholder, if there are no auto fields at all to optimize
            /// allocation time and memory usage
            /// </summary>
            internal static readonly IAutoField[] noAutoFields = new IAutoField[0];

            /// <returns> magic cast to placeholder which well never fail at runtime,
            ///         because the array is empty anyway </returns>
            public static IAutoField[] NoAutoFields
            {
                get
                {
                    return noAutoFields;
                }
            }

            /// <summary>
            /// all fields that hold actual data
            /// 
            /// @note stores fields at index "f.index-1"
            /// </summary>
            internal readonly List<AbstractFieldDeclaration> dataFields;

            public abstract StaticFieldIterator fields();

            public abstract FieldIterator allFields();

            /// <summary>
            /// The block layout of instances of this pool.
            /// </summary>
            internal readonly List<Block> blocks = new List<Block>();

            /// <summary>
            /// internal use only!
            /// </summary>
            public abstract List<Block> Blocks { get; }

            /// <summary>
            /// internal use only!
            /// </summary>
            public abstract Block lastBlock();

            /// <summary>
            /// All stored objects, which have exactly the type T. Objects are stored as
            /// arrays of field entries. The types of the respective fields can be
            /// retrieved using the fieldTypes map.
            /// </summary>
            internal readonly List<object> newObjects = new List<object>();

            /// <summary>
            /// retrieve a new object
            /// </summary>
            /// <param name="index"> in [0;#newObjectsSize()[ </param>
            /// <returns> the new object at the given position </returns>
            public abstract object newObject(int index);

            /// <summary>
            /// Ensures that at least capacity many new objects can be stored in this
            /// pool without moving references.
            /// </summary>
            public abstract void hintNewObjectsSize(int capacity);

            internal abstract int newDynamicInstancesSize();

            /// <summary>
            /// Number of static instances of T in data. Managed by read/compress.
            /// </summary>
            internal int staticDataInstances;

            /// <summary>
            /// the number of instances of exactly this type, excluding sub-types
            /// </summary>
            /// <returns> size excluding subtypes </returns>
            public abstract int staticSize();

            /// <summary>
            /// storage pools can be fixed, i.e. no dynamic instances can be added to the
            /// pool. Fixing a pool requires that it does not contain a new object.
            /// Fixing a pool will fix subpools as well. Un-fixing a pool will un-fix
            /// super pools as well, thus being fixed is a transitive property over the
            /// sub pool relation. Pools will be fixed by flush operations.
            /// </summary>
            internal bool @fixed = false;
            /// <summary>
            /// size that is only valid in fixed state
            /// </summary>
            internal int cachedSize;

            /// <summary>
            /// number of deleted objects in this state
            /// </summary>
            internal int deletedCount = 0;

            /// <summary>
            /// !!internal use only!!
            /// </summary>
            public abstract bool Fixed { get; }

            /// <summary>
            /// fix all pool sizes
            /// 
            /// @note this may change the result of size(), because from now on, the
            ///       deleted objects will be taken into account
            /// </summary>
            internal static void fix(List<AbstractStoragePool> pools)
            {
                // set cached size to static size
                foreach (AbstractStoragePool p in pools)
                {

                    // take deletions into account
                    p.cachedSize = p.staticSize() - p.deletedCount;
                    p.@fixed = true;
                }

                // bubble up cached sizes to parents
                for (int i = pools.Count - 1; i >= 0; i--)
                {
                    AbstractStoragePool p = pools[i];
                    if (null != p.superPool)
                    {
                        p.superPool.cachedSize += p.cachedSize;
                    }
                }
            }

            /// <summary>
            /// unset fixed status
            /// </summary>
            public static void unfix(List<AbstractStoragePool> pools)
            {
                foreach (AbstractStoragePool p in pools)
                {
                    p.@fixed = false;
                }
            }

            public abstract string Name { get; }

            public abstract string superName();

            protected AbstractStoragePool(int poolIndex, string name, AbstractStoragePool superPool, string[] knownFields, IAutoField[] autoFields) : base(32 + poolIndex)
            {

                this.name = name;
                this.superPool = superPool;
                if (null == superPool)
                {
                    typeHierarchyHeight = 0;
                    basePool = (IBasePool)this;
                }
                else
                {
                    typeHierarchyHeight = superPool.typeHierarchyHeight + 1;
                    basePool = superPool.basePool;
                }
                this.knownFields = knownFields;
                dataFields = new List<AbstractFieldDeclaration>(knownFields.Length);

                this.autoFields = autoFields;
            }

            /// <returns> the instance matching argument skill id </returns>
            public abstract object getByID(int ID);

            /// <returns> size including subtypes </returns>
            public abstract int size();

            public abstract Stream stream();

            /// <summary>
            /// Add an existing instance as a new objects.
            /// 
            /// @note Do not use objects managed by other skill files.
            /// </summary>
            public abstract void add(object e);

            /// <summary>
            /// Delete shall only be called from skill state
            /// </summary>
            /// <param name="target"> the object to be deleted
            public abstract void delete(SkillObject target);

            public abstract SkillFile Owner { get; set; }

            public abstract IEnumerable iterator();

            public abstract IEnumerator GetEnumerator();

            public abstract object make();

            public abstract void allocateInstances(Block last);

            internal abstract void updateAfterCompress(int[] lbpoMap);

            /// <summary>
            /// internal use only! adds an unknown field
            /// </summary>
            public abstract AbstractFieldDeclaration addField<R>(FieldType type, string name);

            /// <summary>
            /// used internally for state allocation
            /// </summary>
            public abstract void addKnownField(string name, StringType @string, Annotation annotation);

            /// <summary>
            /// used internally for type forest construction
            /// </summary>
            public abstract AbstractStoragePool makeSubPool(int index, string name);

            /// <summary>
            /// called after a prepare append operation to write empty the new objects
            /// buffer and to set blocks correctly
            /// </summary>
            internal abstract void updateAfterPrepareAppend(int[] lbpoMap, Dictionary<AbstractFieldDeclaration, Chunk> chunkMap);

            public abstract override string ToString();
        }
    }
}