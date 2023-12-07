namespace DOTSCore.AssetManager
{
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Entities.Serialization;

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct EntityPrefabPoolSystem : ISystem
    {
        public struct Singleton : IComponentData
        {
            internal NativeHashMap<FixedString64Bytes, EntityPrefabReference> IDToEntityPrefabReference;
            public NativeHashMap<FixedString64Bytes, Entity>                IDToPrefabEntity;
        }
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EntityPrefabReferenceElement>();
            state.EntityManager.AddComponentData(state.SystemHandle, new Singleton()
            {
                IDToEntityPrefabReference = new NativeHashMap<FixedString64Bytes, EntityPrefabReference>(0, Allocator.Persistent),
                IDToPrefabEntity         = new NativeHashMap<FixedString64Bytes, Entity>(0, Allocator.Persistent)
            });
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            ref var singleton = ref SystemAPI.GetSingletonRW<Singleton>().ValueRW;
            //map entity reference
            foreach (var buffer in SystemAPI.Query<DynamicBuffer<EntityPrefabReferenceElement>>())
            {
                foreach (var entityPrefabReferenceElement in buffer)
                {
                    singleton.IDToEntityPrefabReference.Add(entityPrefabReferenceElement.PrefabName, entityPrefabReferenceElement.Prefab);
                }

            }
            state.EntityManager.DestroyEntity(SystemAPI.QueryBuilder().WithAll<EntityPrefabReferenceElement>().Build());

        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            ref var singleton = ref SystemAPI.GetSingletonRW<Singleton>().ValueRW;
            singleton.IDToEntityPrefabReference.Dispose();
            singleton.IDToPrefabEntity.Dispose();
        }
    }
}