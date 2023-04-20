namespace DOTSCore.Extension
{
    using DOTSCore.CommonSystems.Components;
    using DOTSCore.Group;
    using DOTSCore.World;
    using GameFoundation.Scripts.Utilities.Extension;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Transforms;

    public static class DotsExtension
    {
        public static void SetParent(this EntityManager entityManager, Entity entity, Entity parentEntity)
        {
            entityManager.AddComponentData(entity, new Parent() { Value = parentEntity });
        }

        public static void SetParent(this EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity, Entity parentEntity)
        {
            ecb.AddComponent(index, entity, new Parent() { Value = parentEntity });
        }

        public static void RemoveParent(this EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
        {
            ecb.RemoveComponent<Parent>(index, entity);
        }

        public static void AddChildren(this EntityCommandBuffer.ParallelWriter ecb, int index, Entity rootEntity, NativeList<Entity> children)
        {
            var cacheLinkEntityBuffer = ecb.AddBuffer<LinkedEntityGroup>(index, rootEntity);
            var childEntityBuffer     = ecb.AddBuffer<Child>(index, rootEntity);
            cacheLinkEntityBuffer.Add(new LinkedEntityGroup() { Value = rootEntity });
            foreach (var child in children)
            {
                ecb.SetParent(index, child, rootEntity);
                cacheLinkEntityBuffer.Add(new LinkedEntityGroup() { Value = child });
                childEntityBuffer.Add(new Child() { Value                 = child });
            }
        }

        public static ComponentLookup<T> UpdateComponentLookup<T>(ref this ComponentLookup<T> lookup, SystemBase systemBase) where T : unmanaged, IComponentData
        {
            lookup.Update(systemBase);
            return lookup;
        }

        public static BufferLookup<T> UpdateBufferLookup<T>(ref this BufferLookup<T> lookup, SystemBase systemBase) where T : unmanaged, IBufferElementData
        {
            lookup.Update(systemBase);
            return lookup;
        }
        
        public static Entity CreateNotifyEntity(this EntityManager entityManager) { return entityManager.CreateEntity(typeof(NotifyComponentTag)); }

        public static Entity CreateNotifyEntity(this EntityCommandBuffer.ParallelWriter ecb, int entityInQueryIndex)
        {
            var result = ecb.CreateEntity(entityInQueryIndex);
            ecb.AddComponent<NotifyComponentTag>(entityInQueryIndex, result);
            return result;
        }

        public static void RequestChangeGameState(this EntityManager entityManager, FixedString64Bytes nextState, bool isForce = false)
        {
            var gameStateEntity = entityManager.GetSingletonEntity<CurrentGameState>();

            entityManager.SetComponentData(gameStateEntity, new RequestChangeGameState()
            {
                NextState = nextState,
                IsForce   = isForce
            });
            entityManager.SetComponentEnabled<RequestChangeGameState>(gameStateEntity, true);
        }
    }
}