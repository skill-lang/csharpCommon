using System.Collections;

using de.ust.skill.common.csharp.@internal.parts;

namespace de.ust.skill.common.csharp
{
    namespace @internal
    {

        /// <summary>
        /// Iterates efficiently over static instances of a pool.
        /// 
        /// @author Simon Glaub, Timm Felden
        /// </summary>
        public class StaticDataIterator<T> : IEnumerable where T : SkillObject
        {

            // ! target pool
            AbstractStoragePool p;

            int secondIndex;
            int lastBlock;
            int index;
            int last;

            public StaticDataIterator(AbstractStoragePool storagePool)
            {
                Constructor(storagePool);
            }

            public void Constructor(AbstractStoragePool storagePool)
            {
                p = storagePool;
                lastBlock = storagePool.Blocks.Count;
                // @note other members are zero-allocated

                // find first valid position
                while (index == last && secondIndex < lastBlock)
                {
                    Block b = p.Blocks[secondIndex];
                    index = b.bpo;
                    last = index + b.staticCount;
                    secondIndex++;
                }
                // mode switch, if there is no other block
                if (index == last && secondIndex == lastBlock)
                {
                    secondIndex++;
                    index = 0;
                    last = p.newObjects.Count;
                }
            }

            public bool hasNext()
            {
                return index != last;
            }

            public T next()
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
                            last = index + b.staticCount;
                            secondIndex++;
                        }
                        // mode switch, if there is no other block
                        if (index == last && secondIndex == lastBlock)
                        {
                            secondIndex++;
                            index = 0;
                            last = p.newObjects.Count;
                        }
                    }
                    return r;
                }

                r = (T)p.newObject(index);
                index++;
                return r;
            }


            private class Enumerator : IEnumerator
            {
                StaticDataIterator<T> container;
                T current;

                public Enumerator(StaticDataIterator<T> container)
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
                        current = container.next();
                    }
                    return hasNext;
                }

                public void Reset()
                {
                    container.Constructor(container.p);
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