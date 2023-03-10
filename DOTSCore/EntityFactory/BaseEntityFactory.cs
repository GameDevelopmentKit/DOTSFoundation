using Unity.Entities;

namespace DOTSCore.EntityFactory
{
    using Unity.Mathematics;
    using Unity.Transforms;

    public abstract class BaseEntityFactory<TData> : IEntityFactory<TData>
    {
        public virtual Entity CreateEntity(EntityManager entityManager, TData data)
        {
            var entity = entityManager.CreateEntity();
            entityManager.AddComponentData(entity, LocalTransform.Identity);
            entityManager.AddComponentData(entity, WorldTransform.Identity);
            entityManager.AddComponentData(entity, new LocalToWorld() { Value = float4x4.identity });
            this.InitComponents(entityManager, entity, data);

            return entity;
        }

        protected abstract void InitComponents(EntityManager entityManager, Entity entity, TData data);
    }

    public abstract class BaseEntityPrefabFactory<TData> : BaseEntityFactory<TData>
    {
        public override Entity CreateEntity(EntityManager entityManager, TData data)
        {
            var entity = base.CreateEntity(entityManager, data);
            entityManager.AddComponent<Prefab>(entity);
            return entity;
        }
    }
}