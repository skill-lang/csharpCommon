namespace de.ust.skill.common.csharp
{
    namespace @internal
    {
        /// <summary>
        /// named types store a reference to their type, so that they can be distinguished from another
        /// 
        /// @author Simon Glaub, Timm Felden
        /// </summary>
        public interface NamedType
        {

            /// <returns> the pool that is managing instances of this type </returns>
            AbstractStoragePool ΤPool { get; }
        }
    }
}