namespace DOTSCore.CommonSystems.Systems
{
    using DOTSCore.CommonSystems.Components;
    using DOTSCore.Extension;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct SpawnVisualEntitySystem : ISystem, ISystemStartStop
    {
        EntityQuery                                       entityQuery;
        private NativeHashMap<FixedString64Bytes, Entity> nameToPrefabEntity;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PrefabPoolElement>();
            this.entityQuery = SystemAPI.QueryBuilder().WithAll<ViewPrefabEntityComponent>().WithNone<VisualEntityLink>().Build();
            state.RequireForUpdate(this.entityQuery);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (this.entityQuery.IsEmpty) return;
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            new SpawnEntitiesJob()
            {
                Ecb                     = ecb,
                LinkedEntityGroupLookup = SystemAPI.GetBufferLookup<LinkedEntityGroup>(true),
                NameToPrefabEntity      = this.nameToPrefabEntity
            }.ScheduleParallel(this.entityQuery);
        }
        public void OnStartRunning(ref SystemState state)
        {
            var prefabBuffer = SystemAPI.GetSingletonBuffer<PrefabPoolElement>();
            this.nameToPrefabEntity = new NativeHashMap<FixedString64Bytes, Entity>(prefabBuffer.Length, Allocator.Persistent);
            foreach (var prefabPoolElement in prefabBuffer)
            {
                this.nameToPrefabEntity.Add(prefabPoolElement.PrefabName, prefabPoolElement.Prefab);
            }
        }
        public void OnStopRunning(ref SystemState state) { }
    }

    [BurstCompile]
    public partial struct SpawnEntitiesJob : IJobEntity
    {
        public            EntityCommandBuffer.ParallelWriter        Ecb;
        [ReadOnly] public BufferLookup<LinkedEntityGroup>           LinkedEntityGroupLookup;
        [ReadOnly] public NativeHashMap<FixedString64Bytes, Entity> NameToPrefabEntity;
        private void Execute(Entity logicEntity, [EntityIndexInQuery] int index, in ViewPrefabEntityComponent viewPrefabEntity)
        {
            var visualEntity = this.Ecb.Instantiate(index, NameToPrefabEntity[viewPrefabEntity.Value]);
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