namespace GASCore.Systems.TargetDetectionSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.LogicEffectSystems.Components;
    using GASCore.Systems.StatSystems.Components;
    using GASCore.Systems.TargetDetectionSystems.Components;
    using GASCore.Systems.TimelineSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityTimelineGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct FindTargetWithStatsSystem : ISystem
    {
        private EntityQuery                      entityQuery;
        private ComponentLookup<StatNameToIndex> statNameToIndexLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            using var queryBuilder = new EntityQueryBuilder(Allocator.Temp).WithAll<TeamOwnerId, StatNameToIndex>();
            this.entityQuery           = state.GetEntityQuery(queryBuilder);
            this.statNameToIndexLookup = state.GetComponentLookup<StatNameToIndex>(true);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var entities = this.entityQuery.ToEntityListAsync(state.WorldUpdateAllocator, state.Dependency, out var queryJob);
            this.statNameToIndexLookup.Update(ref state);

            state.Dependency = new FindTargetWithStatsJob()
            {
                Entities              = entities,
                StatNameToIndexLookup = this.statNameToIndexLookup,
            }.ScheduleParallel(queryJob);
        }
    }

    [WithChangeFilter(typeof(TargetWithStatElement))]
    [BurstCompile]
    public partial struct FindTargetWithStatsJob : IJobEntity
    {
        [ReadOnly] public NativeList<Entity>               Entities;
        [ReadOnly] public ComponentLookup<StatNameToIndex> StatNameToIndexLookup;

        private void Execute(
            in DynamicBuffer<TargetWithStatElement> statNames,
            ref DynamicBuffer<TargetableElement> targets,
            ref DynamicBuffer<CompletedTriggerElement> completedTriggerBuffer)
        {
            foreach (var target in this.Entities)
            {
                var statNameToIndex = this.StatNameToIndexLookup[target].BlobValue.Value;

                foreach (var statName in statNames)
                {
                    if (statNameToIndex.ContainsKey(statName.Value))
                    {
                        targets.Add(new TargetableElement() { Value = target });
                        break;
                    }
                }
            }

            // count as complete even if no target found
            completedTriggerBuffer.Add(new CompletedTriggerElement() { Index = TypeManager.GetTypeIndex<TargetWithStatElement>() });
        }
    }
}