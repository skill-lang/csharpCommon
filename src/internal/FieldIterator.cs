using System.Collections;

namespace de.ust.skill.common.csharp
{
    namespace @internal
    {

        public sealed class FieldIterator : IEnumerable
        {

            private AbstractStoragePool p;
            private int i;

            public FieldIterator(AbstractStoragePool p)
            {  
                Constructor(p);
            }

            public void Constructor(AbstractStoragePool p)
            {
                this.p = p;
                i = -p.autoFields.Length;
                while (this.p != null && i == 0 && 0 == this.p.dataFields.Count)
                {
                    this.p = this.p.superPool;
                    if (this.p != null)
                    {
                        i = -this.p.autoFields.Length;
                    }
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
                    do
                    {
                        p = p.superPool;
                        if (p != null)
                        {
                            i = -p.autoFields.Length;
                        }
                    } while (p != null && i == 0 && 0 == p.dataFields.Count);
                }
                return f;
            }


            private class Enumerator : IEnumerator
            {
                FieldIterator container;
                AbstractFieldDeclaration current;

                public Enumerator(FieldIterator container)
                {
                    this.container = container;
                    current = null;
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