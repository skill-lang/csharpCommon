using System.Collections;

namespace de.ust.skill.common.csharp
{
    namespace @internal
    {

        /// <summary>
        /// Iterates efficiently over dynamic instances of a pool in type order.
        /// 
        /// @author Simon Glaub, Timm Felden
        /// @note cast required to work around type system
        /// </summary>
        public class TypeOrderIterator<T, B> : IEnumerable where T : B where B : SkillObject
        {

            TypeHierarchyIterator<T, B> ts;
            StaticDataIterator<T> @is;

            public TypeOrderIterator(StoragePool<T, B> storagePool)
            { 
                Constructor(new TypeHierarchyIterator<T, B>(storagePool));
            }

            public void Constructor(TypeHierarchyIterator<T, B> ts)
            {
                this.ts = ts;
                while (ts.hasNext())
                {
                    AbstractStoragePool t = ts.next();
                    if (0 != t.staticSize())
                    {
                        @is = new StaticDataIterator<T>(t);
                        break;
                    }
                }
            }

            public bool hasNext()
            {
                return @is != null && @is.hasNext();
            }

            public T next()
            {
                T result = @is.next();
                if (!@is.hasNext())
                {
                    while (ts.hasNext())
                    {
                        AbstractStoragePool t = ts.next();
                        if (0 != t.staticSize())
                        {
                            @is = new StaticDataIterator<T>(t);
                            break;
                        }
                    }
                }
                return result;
            }

            private class Enumerator : IEnumerator
            {
                TypeOrderIterator<T, B> container;
                T current;

                public Enumerator(TypeOrderIterator<T, B> container)
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
                    container.Constructor(container.ts);
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