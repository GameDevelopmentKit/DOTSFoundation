namespace DOTSCore.Extension
{
    using DOTSCore.CommonSystems.Components;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Transforms;

    public static class DotsExtension
    {
        public static void SetParent(this EntityManager entityManager, Entity entity, Entity parentEntity) { entityManager.AddComponentData(entity, new Parent() { Value = parentEntity }); }

        public static void SetParent(this EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity, Entity parentEntity)
        {
            ecb.AddComponent(index, entity, new Parent() { Value = parentEntity });
        }

        public static void RemoveParent(this EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity) { ecb.RemoveComponent<Parent>(index, entity); }

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

        public static void AddEnableableComponent<T>(this EntityManager entityManager, Entity entity, T componentData, bool activeValue = false)
            where T : unmanaged, IComponentData, IEnableableComponent
        {
            entityManager.AddComponentData(entity, componentData);
            entityManager.SetComponentEnabled<T>(entity, activeValue);
        }

        public static void AddEnableableComponentTag<T>(this EntityManager entityManager, Entity entity, bool activeValue = false)
            where T : unmanaged, IComponentData, IEnableableComponent
        {
            entityManager.AddComponent<T>(entity);
            entityManager.SetComponentEnabled<T>(entity, activeValue);
        }

        public static void AddEnableableComponentTag<T>(this EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity, bool activeValue = false)
            where T : unmanaged, IComponentData, IEnableableComponent
        {
            ecb.AddComponent<T>(index, entity);
            ecb.SetComponentEnabled<T>(index, entity, activeValue);
        }
        
        public static void AddEnableableComponent<T>(this EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity, T componentData, bool activeValue = false)
            where T : unmanaged, IComponentData, IEnableableComponent
        {
            ecb.AddComponent(index, entity, componentData);
            ecb.SetComponentEnabled<T>(index, entity, activeValue);
        }
        
        /// <summary>Gets the singleton data of the specified component type.</summary>
        public static T GetSingleton<T>(this EntityManager entityManager) where T : unmanaged, IComponentData
            => entityManager.CreateEntityQuery(typeof(T)).GetSingleton<T>();
        
        public static Entity GetSingletonEntity<T>(this EntityManager entityManager) where T : unmanaged, IComponentData
            => entityManager.CreateEntityQuery(typeof(T)).GetSingletonEntity();
    }
}