namespace DOTSCore.AssetManager
{
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Scenes;
    using UnityEngine;

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct LoadPrefabEntitySystem : ISystem
    {
        private NativeHashMap<FixedString64Bytes, Entity> loadingEntityMap;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndInitializationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<EntityPrefabPoolSystem.Singleton>();
            state.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<EntityPrefabReferenceId>().WithNone<PrefabLoadResult>().WithOptions(EntityQueryOptions.IncludePrefab)
                .Build());
            this.loadingEntityMap = new NativeHashMap<FixedString64Bytes, Entity>(0, Allocator.Persistent);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { this.loadingEntityMap.Dispose(); }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            ref var prefabPoolSystem = ref SystemAPI.GetSingletonRW<EntityPrefabPoolSystem.Singleton>().ValueRW;
            if (prefabPoolSystem.IDToEntityPrefabReference.IsEmpty) return;

            var ecbSystem = SystemAPI.GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>();
            var ecb       = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);

            foreach (var (prefabId, entity) in SystemAPI.Query<RefRO<EntityPrefabReferenceId>>().WithNone<PrefabLoadResult>().WithEntityAccess().WithOptions(EntityQueryOptions.IncludePrefab))
            {
                if (this.loadingEntityMap.TryGetValue(prefabId.ValueRO.Value, out var loadingEntity))
                {
                    var prefabStreamingState = SceneSystem.GetSceneStreamingState(state.WorldUnmanaged, loadingEntity);
                    if (prefabStreamingState == SceneSystem.SceneStreamingState.LoadedSuccessfully)
                    {
                        var prefabRoot = state.EntityManager.GetComponentData<PrefabRoot>(loadingEntity);
                        prefabPoolSystem.IDToPrefabEntity[prefabId.ValueRO.Value] = prefabRoot.Root;
                        this.loadingEntityMap.Remove(prefabId.ValueRO.Value);
                    }
                    else
                    {
                        continue;
                    }
                }

                if (prefabPoolSystem.IDToPrefabEntity.TryGetValue(prefabId.ValueRO.Value, out var prefabEntity))
                {
                    ecb.AddComponent(entity, new PrefabLoadResult
                    {
                        PrefabRoot = prefabEntity
                    });
                    continue;
                }

                if (prefabPoolSystem.IDToEntityPrefabReference.TryGetValue(prefabId.ValueRO.Value, out var prefabReference))
                {
                    if (prefabReference.IsReferenceValid)
                    {
                        this.loadingEntityMap.Add(prefabId.ValueRO.Value, SceneSystem.LoadPrefabAsync(state.WorldUnmanaged, prefabReference));
                        continue;
                    }
                }

                Debug.LogError($"Prefab {prefabId.ValueRO.Value} not found");
            }
        }
    }
}