using Unity.Entities;

namespace DOTSCore.EntityFactory
{
    using Unity.Mathematics;
    using Unity.Transforms;

    public abstract class BaseEntityFactory<TData> : IEntityFactory<TData>
    {
        public virtual Entity CreateEntity(EntityManager entityManager, TData data, params ComponentType[] types)
        {
            var entity = entityManager.CreateEntity(types);

            this.InitComponents(entityManager, entity, data);

            return entity;
        }

        protected abstract void InitComponents(EntityManager entityManager, Entity entity, TData data);
    }

    public abstract class BaseEntityPrefabFactory<TData> : BaseEntityFactory<TData>
    {
        public override Entity CreateEntity(EntityManager ecb, TData data, params ComponentType[] types)
        {
            return base.CreateEntity(ecb, data, typeof(Prefab), typeof(Translation), typeof(LocalToWorld), typeof(Rotation));
        }
    }
}