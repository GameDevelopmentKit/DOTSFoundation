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
        public static T AddSystemGroup<T>(this BaseWorld baseWorld) where T : BaseSystemGroup
        {
            var systemGroup = baseWorld.CreateAndDiSystem<T>();
            return systemGroup;
        }

        /// <summary>
        /// Creates a new managed system and adds it to this BaseSystemGroup's update list.
        /// If system sorting is disabled, the update order is based on the order the systems are added.
        /// If the system already exists in the world, that system is added to the update list and returned instead.
        /// </summary>
        /// <typeparam name="T">The type of managed system to create</typeparam>
        /// <returns>The managed system added</returns>
        public static T AddSystem<T>(this BaseSystemGroup systemGroup) where T : ComponentSystemBase
        {
            var system = systemGroup.World.CreateAndDiSystem<T>();
            systemGroup.AddSystemToUpdateList(system);
            return system;
        }

        /// <summary>
        /// Creates a new unmanaged system and adds it to this BaseSystemGroup's update list.
        /// If system sorting is disabled, the update order is based on the order the systems are added.
        /// If the system already exists in the world, that system is added to the update list and returned instead.
        /// </summary>
        /// <typeparam name="T">The type of unmanaged system to create</typeparam>
        /// <returns>The unmanaged system added</returns>
        public static T AddUnmanagedSystem<T>(this BaseSystemGroup systemGroup) where T : unmanaged, DOTSCore.Interfaces.ISystem
        {
            var systemHandle = systemGroup.World.CreateSystem<T>();
            var systemRef    = systemGroup.World.Unmanaged.GetUnsafeSystemRef<T>(systemHandle);
            systemGroup.GetCurrentContainer().Inject(systemRef);
            systemRef.Initialize();
            systemGroup.AddSystemToUpdateList(systemHandle);
            return systemRef;
        }

        public static void BindTo<T>(this ComponentSystemBase systemBase) where T : ComponentSystemGroup
        {
            var systemGroup = systemBase.World.GetOrCreateSystemManaged(typeof(T)) as ComponentSystemGroup;
            systemGroup?.AddSystemToUpdateList(systemBase);
        }

        private static T CreateAndDiSystem<T>(this BaseWorld baseWorld) where T : ComponentSystemBase
        {
            var system = TypeManager.ConstructSystem<T>();
            baseWorld.GetCurrentContainer().Inject(system);
            baseWorld.AddSystemManaged(system);
            return system;
        }

        public static void SetParent(this EntityManager entityManager, Entity entity, Entity parentEntity)
        {
            entityManager.AddComponentData(entity, new Parent() { Value = parentEntity });
            entityManager.AddComponent<ParentTransform>(entity);
        }

        public static void SetParent(this EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity, Entity parentEntity)
        {
            ecb.AddComponent(index, entity, new Parent() { Value = parentEntity });
            ecb.AddComponent<ParentTransform>(index, entity);
        }

        public static void RemoveParent(this EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
        {
            ecb.RemoveComponent<Parent>(index, entity);
            ecb.RemoveComponent<ParentTransform>(index, entity);
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