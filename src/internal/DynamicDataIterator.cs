using System.Collections;

using de.ust.skill.common.csharp.@internal.parts;

namespace de.ust.skill.common.csharp
{
    namespace @internal
    {

        /// <summary>
        /// Iterates efficiently over dynamic instances of a pool.
        /// 
        /// First phase will iterate over all blocks of the pool. The second phase will
        /// iterate over all dynamic instances of the pool.
        /// 
        /// @author Simon Glaub, Timm Felden
        /// </summary>
        public sealed class DynamicDataIterator<T, B> : IEnumerable, IDynamicDataIterator where T : B where B : SkillObject
        {

            AbstractStoragePool p;

            int secondIndex;
            int lastBlock;
            int endHeight;
            int index;
            int last;

            public DynamicDataIterator(StoragePool<T, B> storagePool)
            {
                Constructor(storagePool);
            }

            private void Constructor(StoragePool<T, B> storagePool)
            {
                p = storagePool;
                endHeight = p.typeHierarchyHeight;
                lastBlock = p.Blocks.Count;
                // other fields are zero-allocated

                while (index == last && secondIndex < lastBlock)
                {
                    Block b = p.Blocks[secondIndex];
                    index = b.bpo;
                    last = index + b.count;
                    secondIndex++;
                }
                // mode switch, if there is no other block
                if (index == last && secondIndex == lastBlock)
                {
                    secondIndex++;
                    while (null != p)
                    {
                        if (p.newObjects.Count != 0)
                        {
                            index = 0;
                            last = p.newObjects.Count;
                            break;
                        }
                        nextP();
                    }
                }
            }

            public bool hasNext()
            {
                return null != p;
            }

            public object next()
            {
                T r;

                if (secondIndex <= lastBlock)
                {
                    r = (T)p.data[index];
                    index++;
                    if (index == last)
                    {
                        while (index == last && secondIndex < lastBlock)
                        {
                            Block b = p.Blocks[secondIndex];
                            index = b.bpo;
                            last = index + b.count;
                            secondIndex++;
                        }
                        // mode switch, if there is no other block
                        if (index == last && secondIndex == lastBlock)
                        {
                            secondIndex++;
                            while (null != p)
                            {
                                if (p.newObjects.Count != 0)
                                {
                                    index = 0;
                                    last = p.newObjects.Count;
                                    break;
                                }
                                nextP();
                            }
                        }
                    }
                    return r;
                }

                r = (T)p.newObject(index);
                index++;
                if (index == last)
                {
                    do
                    {
                        nextP();
                        if (null == p)
                        {
                            break;
                        }

                        if (p.newObjects.Count != 0)
                        {
                            index = 0;
                            last = p.newObjects.Count;
                            break;
                        }
                    } while (true);
                }
                return r;
            }

            public void nextP()
            {
                AbstractStoragePool n = p.NextPool;
                if (null != n && endHeight < n.typeHierarchyHeight)
                {
                    p = n;
                }
                else
                {
                    p = null;
                }
            }


            private class Enumerator : IEnumerator
            {
                DynamicDataIterator<T, B> container;

                T current;

                public Enumerator(DynamicDataIterator<T, B> container)
                {
                    this.container = container;
                }

                public object Current
                {
                    get
                    {
                        return current;
                    }
                }

                public bool MoveNext()
                {
                    bool hasNext = container.hasNext();
                    if (hasNext)
                    {
                        current = (T)container.next();
                    }
                    return hasNext;
                }

                public void Reset()
                {
                    container.Constructor((StoragePool<T,B>)container.p);
                    current = null;
                }
            }

            public IEnumerator GetEnumerator()
            {
                return new Enumerator(this);
            }
        }
    }
}