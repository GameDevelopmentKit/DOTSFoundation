namespace GASCore.Systems.AbilityMainFlow.Systems.AbilitySystem
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using GASCore.Systems.LogicEffectSystems.Systems;
    using GASCore.Systems.TimelineSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityCleanupSystemGroup))]
    [UpdateBefore(typeof(DestroyTargetSystem))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct CleanupUnusedAbilityEntitiesSystem : ISystem
    {
        private EntityQuery forceCleanupEntityQuery;
        private EntityQuery normalCleanupEntityQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            using var queryBuilder = new EntityQueryBuilder(Allocator.Temp).WithAll<ActivatedStateEntityOwner, ForceCleanupTag>();
            this.forceCleanupEntityQuery = state.GetEntityQuery(queryBuilder);

            queryBuilder.Reset();
            queryBuilder.WithAll<ActivatedStateEntityOwner>().WithNone<InTriggerConditionResolveProcessTag, EndTimeComponent, Duration, IgnoreCleanupTag, ForceCleanupTag>();
            this.normalCleanupEntityQuery = state.GetEntityQuery(queryBuilder);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            new CleanupAbilityActionEntitiesJob()
            {
                Ecb = ecb
            }.ScheduleParallel(normalCleanupEntityQuery);

            new CleanupAbilityActionEntitiesJob()
            {
                Ecb = ecb
            }.ScheduleParallel(forceCleanupEntityQuery);
        }
    }

    [BurstCompile]
    public partial struct CleanupAbilityActionEntitiesJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;
        [BurstCompile]
        void Execute(Entity abilityActionEntity, [EntityIndexInQuery] int entityInQueryIndex, in ActivatedStateEntityOwner activatedStateEntityOwner)
        {
            this.Ecb.RemoveComponent<ActivatedStateEntityOwner>(entityInQueryIndex, abilityActionEntity);
            this.Ecb.DestroyEntity(entityInQueryIndex, abilityActionEntity);
        }
    }

    [UpdateInGroup(typeof(AbilityMainFlowGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct CleanupActivatedStateAbilityEntitiesSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            new CleanupActivatedStateAbilityEntitiesJob()
            {
                Ecb                             = ecb,
                ActivatedStateEntityOwnerLookup = SystemAPI.GetComponentLookup<ActivatedStateEntityOwner>(true)
            }.ScheduleParallel();
        }

        [BurstCompile]
        [WithAll(typeof(AbilityOwner))]
        public partial struct CleanupActivatedStateAbilityEntitiesJob : IJobEntity
        {
            [ReadOnly] public ComponentLookup<ActivatedStateEntityOwner> ActivatedStateEntityOwnerLookup;
            public            EntityCommandBuffer.ParallelWriter         Ecb;

            void Execute(Entity abilityActivatedStateEntity, [EntityIndexInQuery] int entityInQueryIndex, ref DynamicBuffer<LinkedEntityGroup> linkedEntityGroups)
            {
                for (var index = 0; index < linkedEntityGroups.Length; index++)
                {
                    var linkedEntity = linkedEntityGroups[index];

                    if (this.ActivatedStateEntityOwnerLookup.HasComponent(linkedEntity.Value) || linkedEntity.Value == abilityActivatedStateEntity) continue;
                    linkedEntityGroups.RemoveAtSwapBack(index);
                }

                if (linkedEntityGroups.Length <= 1)
                {
                    this.Ecb.DestroyEntity(entityInQueryIndex, abilityActivatedStateEntity);
                }
            }
        }
    }
}