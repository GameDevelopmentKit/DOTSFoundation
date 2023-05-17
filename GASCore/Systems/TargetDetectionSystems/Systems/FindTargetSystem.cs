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

        [BurstCompile]
        public void OnCreate(ref SystemState state) { this.entityQuery = SystemAPI.QueryBuilder().WithAll<LocalToWorld, StatNameToIndex, TeamOwnerId>().WithNone<UntargetableTag>().Build(); }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var entities = this.entityQuery.ToEntityListAsync(state.WorldUpdateAllocator, state.Dependency, out var queryJob);

            var recycleJob = new RecycleFindTargetComponentJob()
            {
                FindTargetLookup = SystemAPI.GetComponentLookup<FindTargetComponent>()
            }.ScheduleParallel(state.Dependency);

            state.Dependency = new FindTargetJob
            {
                Entities = entities
            }.ScheduleParallel(JobHandle.CombineDependencies(queryJob, recycleJob));
        }
    }

    [WithDisabled(typeof(FindTargetComponent))]
    [BurstCompile]
    public partial struct RecycleFindTargetComponentJob : IJobEntity
    {
        [NativeDisableParallelForRestriction] public ComponentLookup<FindTargetComponent> FindTargetLookup;
        private void Execute(
            Entity entity,
            in TriggerConditionAmount triggersAmount,
            in DynamicBuffer<CompletedTriggerElement> completedTriggers)
        {
            var findTargetTrigger = TypeManager.GetTypeIndex<FindTargetComponent>().Index;
            foreach (var trigger in completedTriggers)
            {
                // completed, wait for recycle
                if (trigger == findTargetTrigger) return;
            }

            // wait for other triggers
            if (this.FindTargetLookup[entity].WaitForOtherTriggers && completedTriggers.Length < triggersAmount - 1) return;
            this.FindTargetLookup.SetComponentEnabled(entity, true);
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