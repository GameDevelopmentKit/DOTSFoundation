using Unity.Entities;

namespace DOTSCore.EntityFactory
{
    public interface IEntityFactory { }

    public interface IEntityFactory<in TData> : IEntityFactory
    {
        /// <summary>
        /// Create entity with default components for it.
        /// </summary>
        /// <param name="entityManager"></param>
        /// <param name="data"></param>
        /// <param name="types">The types of components to add to the new entity</param>
        /// <returns>Entity</returns>
        Entity CreateEntity(EntityManager entityManager, TData data, params ComponentType[] types);
    }

    public interface IEntityFactoryByEcb<in TData> : IEntityFactory
    {
        /// <summary>
        /// Create entity with default components for it by using EntityCommandBuffer
        /// </summary>
        /// <param name="ecb"></param>
        /// <param name="index"></param>
        /// <param name="data"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        Entity CreateEntity(EntityCommandBuffer.ParallelWriter ecb, int index, TData data, params ComponentType[] types);
    }
}