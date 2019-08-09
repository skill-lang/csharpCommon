using System;
using System.Threading;
using System.Collections.Generic;

using SkillFile = de.ust.skill.common.csharp.api.SkillFile;
using de.ust.skill.common.csharp.@internal.parts;
using de.ust.skill.common.csharp.@internal.fieldDeclarations;

namespace de.ust.skill.common.csharp
{
    namespace @internal
    {

        /// <summary>
        /// The base of a type hierarchy. Contains optimized representations of data
        /// compared to sub pools.
        /// 
        /// @author Simon Glaub, Timm Felden </summary>
        /// @param <T> </param>
        public class BasePool<T> : StoragePool<T, T>, IBasePool where T : SkillObject
        {

            /// <summary>
            /// workaround for fucked-up generic array types
            /// </summary>
            /// <returns> an empty array that is used as initial value of data
            /// @note has to be overridden by each concrete base pool </returns>
            protected virtual T[] newArray(int size)
            {
                return new T[size];
            }

            /// <summary>
            /// the owner is set once by the SkillState.finish method!
            /// </summary>
            protected SkillFile owner = null;

            public BasePool(int poolIndex, string name, string[] knownFields, IAutoField[] autoFields) : base(poolIndex, name, null, knownFields, autoFields)
            {
            }

            public override SkillFile Owner
            {
                get
                {
                    return owner;
                }
                set
                {
                    owner = value;
                }
            }

            public int getCachedSize()
            {
                return cachedSize;
            }

            public object[] getData()
            {
                return data;
            }

            /// <summary>
            /// Allocates data and all instances for this pool and all of its sub-pools.
            /// @note invoked once upon state creation before deserialization of field data
            /// </summary>
            /// <param name="barrier">used to synchronize parallel object allocation </param>
            public int performAllocations(Semaphore barrier)
            {
                int reads = 0;

                {
                    // allocate data and link it to sub pools
                    data = newArray(cachedSize);
                    TypeHierarchyIterator<T, T> subs = new TypeHierarchyIterator<T, T>(this);
                    while (subs.hasNext())
                    {
                        subs.next().data = data;
                    }
                }

                {
                    // allocate instances
                    TypeHierarchyIterator<T, T> subs = new TypeHierarchyIterator<T, T>(this);
                    while (subs.hasNext())
                    {
                        AbstractStoragePool s = subs.next();
                        foreach (Block b in s.Blocks)
                        {
                            reads++;
                            ThreadPool.QueueUserWorkItem(new WaitCallback((Object stateInfo) =>
                            {
                                s.allocateInstances(b);
                                barrier.Release();
                            }));
                        }
                    }
                }
                return reads;
            }

            /// <summary>
            /// compress new instances into the data array and update skillIDs
            /// </summary>
            public void compress(int[] lbpoMap)
            {

                {
                    // create our part of the lbpo map
                    int next = 0;
                    TypeHierarchyIterator<T, T> subs = new TypeHierarchyIterator<T, T>(this);

                    while (subs.hasNext())
                    {
                        AbstractStoragePool q = subs.next();

                        int lbpo = lbpoMap[q.TypeID - 32] = next;
                        next += q.staticSize() - q.deletedCount;

                        foreach (AbstractFieldDeclaration f in q.dataFields)
                        {
                            f.resetChunks(lbpo, q.cachedSize);
                        }
                    }
                }

                // from now on, size will take deleted objects into account, thus d may
                // in fact be smaller then data!
                T[] d = newArray(size());
                int p = 0;
                TypeOrderIterator<T, T> @is = (TypeOrderIterator<T, T>)typeOrderIterator();
                while (@is.hasNext())
                {
                    T i = @is.next();
                    if (i.skillID != 0)
                    {
                        d[p++] = i;
                        i.skillID = p;
                    }
                }

                // update after compress for all sub-pools
                data = d;
                {
                    TypeHierarchyIterator<T, T> subs = new TypeHierarchyIterator<T, T>(this);
                    while (subs.hasNext())
                    {
                        subs.next().updateAfterCompress(lbpoMap);
                    }
                }
            }

            public void prepareAppend(int[] lbpoMap, Dictionary<AbstractFieldDeclaration, Chunk> chunkMap)
            {

                {
                    // update LBPO map
                    int next = data.Length;
                    foreach (AbstractStoragePool p in new TypeHierarchyIterator<T, T>(this))
                    {
                        lbpoMap[p.TypeID - 32] = next;
                        next += p.newObjects.Count;
                    }
                }

                bool newInstances = newDynamicInstances().hasNext();

                // check if we have to append at all
                if (!newInstances && !(Blocks.Count == 0) && !(dataFields.Count == 0))
                {
                    bool done = true;
                    foreach (AbstractFieldDeclaration f in dataFields)
                    {
                        if (0 == f.dataChunks.Count)
                        {
                            done = false;
                            break;
                        }
                    }
                    if (done)
                    {
                        return;
                    }
                }

                if (newInstances)
                {
                    // we have to resize
                    T[] d = new T[size()];
                    Array.Copy(data, d, size());
                    int i = data.Length;

                    DynamicNewInstancesIterator<T, T> @is = newDynamicInstances();
                    while (@is.GetEnumerator().MoveNext())
                    {
                        T instance = (T)@is.GetEnumerator().Current;
                        d[i++] = instance;
                        instance.skillID = i;
                    }
                    data = d;
                }

                TypeHierarchyIterator<T, T> ts = new TypeHierarchyIterator<T, T>(this);
                while (ts.hasNext())
                {
                    ts.next().updateAfterPrepareAppend(lbpoMap, chunkMap);
                }
            }
        }
    }
}