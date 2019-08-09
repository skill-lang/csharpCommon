using System.Collections;

namespace de.ust.skill.common.csharp
{
    namespace @internal
    {

        /// <summary>
        /// Iterate over fields of a single pool ignoring super pools.
        /// 
        /// @author Simon Glaub, Timm Felden
        /// 
        /// </summary>
        public sealed class StaticFieldIterator : IEnumerable
        {

            private AbstractStoragePool p;
            private int i;

            internal StaticFieldIterator(AbstractStoragePool p)
            {
                Constructor(p);
            }

            public void Constructor(AbstractStoragePool p)
            {
                if (p.autoFields.Length == 0 && 0 == p.dataFields.Count)
                {
                    this.p = null;
                    i = 0;
                }
                else
                {
                    this.p = p;
                    i = -p.autoFields.Length;
                }
            }

            public bool hasNext()
            {
                return p != null;
            }


            public AbstractFieldDeclaration next()
            {

                AbstractFieldDeclaration f = i < 0 ? (AbstractFieldDeclaration)p.autoFields[-1 - i] : p.dataFields[i];
                i++;
                if (i == p.dataFields.Count)
                {
                    p = null;
                }
                return f;
            }


            private class Enumerator : IEnumerator
            {
                StaticFieldIterator container;
                AbstractFieldDeclaration current;

                public Enumerator(StaticFieldIterator container)
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