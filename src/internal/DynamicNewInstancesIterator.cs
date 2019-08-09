using System.Collections;

namespace de.ust.skill.common.csharp
{
    namespace @internal
    {

        /// <summary>
        /// Iterates efficiently over dynamic new instances of a pool.
        /// 
        /// Like second phase of dynamic data iterator.
        /// 
        /// @author Simon Glaub, Timm Felden
        /// </summary>
        public sealed class DynamicNewInstancesIterator<T, B> : IEnumerable where T : B where B : SkillObject
        {

            TypeHierarchyIterator<T, B> ts;
            private StoragePool<T, B> p;

            int index;
            int last;

            public DynamicNewInstancesIterator(StoragePool<T, B> storagePool)
            {
                Constructor(storagePool);
            }

            public void Constructor(StoragePool<T, B> storagePool)
            {
                p = storagePool;
                ts = new TypeHierarchyIterator<T, B>(storagePool);
                last = storagePool.newObjects.Count;

                while (0 == last && ts.hasNext())
                {
                    ts.next();
                    if (ts.hasNext())
                    {
                        last = ts.get().newObjects.Count;
                    }
                    else
                    {
                        return;
                    }
                }
            }


            public bool hasNext()
            {
                return index != last;
            }

            public T next()
            {
                T rval = (T)ts.get().newObject(index);
                index++;
                if (index == last && ts.hasNext())
                {
                    index = last = 0;
                    do
                    {
                        ts.next();
                        if (ts.hasNext())
                        {
                            last = ts.get().newObjects.Count;
                        }
                        else
                        {
                            return rval;
                        }
                    } while (0 == last && ts.hasNext());
                }
                return rval;
            }


            private class Enumerator : IEnumerator
            {
                DynamicNewInstancesIterator<T, B> container;

                T current;

                public Enumerator(DynamicNewInstancesIterator<T, B> container)
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