namespace GASCore.Systems.CasterSystems.Systems
{
    using DOTSCore.Extension;
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.CasterSystems.Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Scenes;

    [UpdateInGroup(typeof(GameAbilityInitializeSystemGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct InstantiateAbilityEntityPrefabSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginInitializationEntityCommandBufferSystem.Singleton>();
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new WaitToAddAbilityToCasterJob()
            {
                ECB = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
            }.ScheduleParallel();
        }
        
        [BurstCompile]
        public partial struct WaitToAddAbilityToCasterJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter ECB;
            void Execute(Entity loaderEntity,[EntityIndexInQuery] int entityInQueryIndex, in LoadAbilityPrefabData loadAbilityPrefabData, in PrefabLoadResult prefab )
            {
                InstantiateAbilityEntity(this.ECB, entityInQueryIndex, loadAbilityPrefabData.CasterEntity, prefab.PrefabRoot, loadAbilityPrefabData.RequestData);
                this.ECB.DestroyEntity(entityInQueryIndex, loaderEntity);
            }
        }
        
        public static void InstantiateAbilityEntity(EntityCommandBuffer.ParallelWriter ecb, int entityInQueryIndex, Entity casterEntity, Entity abilityPrefab, RequestAddOrUpgradeAbility request)
        {
            var abilityEntity = ecb.Instantiate(entityInQueryIndex, abilityPrefab);
            if (request.IsAddPrefab)
            {
                ecb.AddComponent<Prefab>(entityInQueryIndex, abilityEntity);
            }

            ecb.AppendToBuffer(entityInQueryIndex, casterEntity, new AbilityContainerElement()
            {
                AbilityId       = request.AbilityId,
                Level           = request.Level,
                AbilityInstance = abilityEntity
            });

            ecb.AddComponent(entityInQueryIndex, abilityEntity, new CasterComponent() { Value = casterEntity });
            ecb.SetParent(entityInQueryIndex, abilityEntity, casterEntity);
            ecb.AppendToBuffer(entityInQueryIndex, casterEntity, new LinkedEntityGroup() { Value = abilityEntity });
        }
    }
}