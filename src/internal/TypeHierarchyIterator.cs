using System.Collections;

namespace de.ust.skill.common.csharp
{
    namespace @internal
    {

        /// <summary>
        /// iterates efficiently over the type hierarchy
        /// 
        /// @author Simon Glaub, Timm Felden
        /// </summary>
        public class TypeHierarchyIterator<T, B> : IEnumerable where T : B where B : SkillObject
        {

            AbstractStoragePool p;
            int end;

            public TypeHierarchyIterator(AbstractStoragePool pool)
            {
                Constructor(pool);
            }

            public void Constructor(AbstractStoragePool pool)
            {
                p = pool;
                end = pool.typeHierarchyHeight;
            }

            public bool hasNext()
            {
                return null != p;
            }

            public AbstractStoragePool next()
            {
                AbstractStoragePool r = p;
                AbstractStoragePool n = (AbstractStoragePool)p.NextPool;
                if (null != n && end < n.typeHierarchyHeight)
                {
                    p = n;
                }
                else
                {
                    p = null;
                }
                return r;
            }

            /// <summary>
            /// @note valid, iff hasNext 
            /// </summary>
            /// <returns> the current element </returns>
            public virtual AbstractStoragePool get()
            {
                return p;
            }

            private class Enumerator : IEnumerator
            {
                TypeHierarchyIterator<T, B> container;
                AbstractStoragePool current;

                public Enumerator(TypeHierarchyIterator<T, B> container)
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

            public virtual IEnumerator GetEnumerator()
            {
                return new Enumerator(this);
            }
        }
    }
}