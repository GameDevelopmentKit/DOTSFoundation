namespace GASCore.Systems.TargetDetectionSystems.Systems
{
    using GASCore.Systems.LogicEffectSystems.Components;
    using GASCore.Systems.StatSystems.Components;
    using GASCore.Systems.TargetDetectionSystems.Components;
    using GASCore.Systems.TimelineSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;
    using Unity.Transforms;

    [UpdateInGroup(typeof(FindTargetGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct FindTargetSystem : ISystem
    {
        private EntityQuery entityQuery;

        public void OnCreate(ref SystemState state)
        {
            this.entityQuery = SystemAPI.QueryBuilder().WithAll<LocalToWorld, StatNameToIndex, TeamOwnerId>().Build();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            var entities = this.entityQuery.ToEntityListAsync(state.WorldUpdateAllocator, state.Dependency, out var queryJob);

            var recycleJob = new RecycleFindTargetComponentJob
            {
                Ecb = ecb,
            }.ScheduleParallel(state.Dependency);

            state.Dependency = new FindTargetJob
            {
                Entities = entities,
            }.ScheduleParallel(JobHandle.CombineDependencies(queryJob, recycleJob));
        }
    }

    [WithDisabled(typeof(FindTargetComponent))]
    [BurstCompile]
    public partial struct RecycleFindTargetComponentJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;

        private void Execute(
            Entity entity,
            [EntityIndexInQuery] int index,
            in FindTargetComponent param,
            in TriggerConditionAmount triggersAmount,
            in DynamicBuffer<CompletedTriggerElement> completedTriggers
        )
        {
            var findTargetTrigger = TypeManager.GetTypeIndex<FindTargetComponent>().Index;
            foreach (var trigger in completedTriggers)
            {
                // completed, wait for recycle
                if (trigger == findTargetTrigger) return;
            }

            // wait for other triggers
            if (param.WaitForOtherTriggers && completedTriggers.Length < triggersAmount - 1) return;

            this.Ecb.SetComponentEnabled<FindTargetComponent>(index, entity, true);
        }
    }

    [WithAll(typeof(FindTargetComponent))]
    [BurstCompile]
    public partial struct FindTargetJob : IJobEntity
    {
        [ReadOnly] public NativeList<Entity> Entities;

        private void Execute(ref DynamicBuffer<TargetableElement> targetables)
        {
            targetables.Clear();
            foreach (var entity in this.Entities)
            {
                targetables.Add(entity);
            }
        }
    }
}