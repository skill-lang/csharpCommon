using System.Collections;

namespace de.ust.skill.common.csharp
{
    namespace @internal
    {

        /// <summary>
        /// Returns all instances for an interface pool.
        /// 
        /// @author Simon Glaub, Timm Felden
        /// 
        /// </summary>
        public sealed class InterfaceIterator<T> : IEnumerable
        {

            private AbstractStoragePool[] ps;
            private int i;
            private IDynamicDataIterator xs;

            public InterfaceIterator(AbstractStoragePool[] realizations)
            {
                Constructor(realizations);
            }

            public void Constructor(AbstractStoragePool[] realizations)
            {
                ps = realizations;
                while (i < ps.Length)
                {
                    xs = (IDynamicDataIterator)ps[i++].iterator();
                }
            }

            public bool hasNext()
            {
                return xs.hasNext();
            }

            public T next()
            {
                T r = (T)xs.next();
                if (!xs.hasNext())
                {
                    while (i < ps.Length)
                    {
                        xs = (IDynamicDataIterator)ps[i++].iterator();
                    }
                }

                return r;
            }


            private class Enumerator : IEnumerator
            {
                InterfaceIterator<T> container;
                T current;

                public Enumerator(InterfaceIterator<T> container)
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
                    container.Constructor(container.ps);
                    current = default(T);
                }
            }

            public IEnumerator GetEnumerator()
            {
                return new Enumerator(this);
            }
        }
    }
}