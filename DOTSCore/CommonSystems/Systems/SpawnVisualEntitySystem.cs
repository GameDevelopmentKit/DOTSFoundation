namespace DOTSCore.CommonSystems.Systems
{
    using DOTSCore.AssetManager;
    using DOTSCore.CommonSystems.Components;
    using DOTSCore.Extension;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Scenes;

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct SpawnVisualEntitySystem : ISystem
    {
        EntityQuery entityQuery;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginInitializationEntityCommandBufferSystem.Singleton>();
            this.entityQuery = SystemAPI.QueryBuilder().WithAll<EntityPrefabReferenceId, PrefabLoadResult>().WithNone<VisualEntityLink>().Build();
            state.RequireForUpdate(this.entityQuery);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (this.entityQuery.IsEmpty) return;
            var ecbSingleton = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            new SpawnEntitiesJob()
            {
                Ecb                     = ecb,
                LinkedEntityGroupLookup = SystemAPI.GetBufferLookup<LinkedEntityGroup>(true),
            }.ScheduleParallel(this.entityQuery);
        }
    }

    [BurstCompile]
    public partial struct SpawnEntitiesJob : IJobEntity
    {
        public            EntityCommandBuffer.ParallelWriter Ecb;
        [ReadOnly] public BufferLookup<LinkedEntityGroup>    LinkedEntityGroupLookup;
        private void Execute(Entity logicEntity, [EntityIndexInQuery] int index, in PrefabLoadResult entityPrefab)
        {
            var visualEntity = this.Ecb.Instantiate(index, entityPrefab.PrefabRoot);
            this.Ecb.AddComponent(index, logicEntity, new VisualEntityLink() { Value = visualEntity });
            this.Ecb.SetParent(index, visualEntity, logicEntity);
            if (!this.LinkedEntityGroupLookup.HasBuffer(logicEntity))
            {
                this.Ecb.AddBuffer<LinkedEntityGroup>(index, logicEntity).Add(new LinkedEntityGroup() { Value = logicEntity });
            }

            this.Ecb.AppendToBuffer(index, logicEntity, new LinkedEntityGroup() { Value = visualEntity });
        }
    }
}