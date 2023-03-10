namespace GASCore.Systems.TargetDetectionSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using GASCore.Systems.StatSystems.Components;
    using GASCore.Systems.TargetDetectionSystems.Components;
    using GASCore.Systems.TimelineSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;

    [UpdateInGroup(typeof(AbilityTimelineGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct FindTargetWithStatsSystem : ISystem
    {
        private EntityQuery                      entityQuery;
        private ComponentLookup<StatNameToIndex> statNameToIndexLookup;
        private ComponentLookup<TagComponent>    tagLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            using var queryBuilder = new EntityQueryBuilder(Allocator.Temp).WithAll<TeamOwnerId, StatNameToIndex>();
            this.entityQuery           = state.GetEntityQuery(queryBuilder);
            this.statNameToIndexLookup = state.GetComponentLookup<StatNameToIndex>(true);
            this.tagLookup             = state.GetComponentLookup<TagComponent>(true);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            this.statNameToIndexLookup.Update(ref state);
            this.tagLookup.Update(ref state);

            var convertJob = new AddKillCountStatNameFromTagJob()
            {
                TagLookup = this.tagLookup,
            }.ScheduleParallel(state.Dependency);

            var entities = this.entityQuery.ToEntityListAsync(state.WorldUpdateAllocator, state.Dependency, out var queryJob);

            state.Dependency = new FindTargetWithStatsJob()
            {
                Entities              = entities,
                StatNameToIndexLookup = this.statNameToIndexLookup,
            }.ScheduleParallel(JobHandle.CombineDependencies(convertJob, queryJob));
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
                var targetable      = true;

                foreach (var statName in statNames)
                {
                    if (!statNameToIndex.ContainsKey(statName))
                    {
                        targetable = false;
                        break;
                    }
                }

                if (targetable)
                {
                    targets.Add(target);
                    // break;
                }
            }

            // count as complete even if no target found
            completedTriggerBuffer.Add(new CompletedTriggerElement() { Index = TypeManager.GetTypeIndex<TargetWithStatElement>() });
        }
    }

    [WithChangeFilter(typeof(TriggerKillCountTag))]
    [BurstCompile]
    public partial struct AddKillCountStatNameFromTagJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<TagComponent> TagLookup;

        private void Execute(in CasterComponent caster, ref DynamicBuffer<TargetWithStatElement> statNames)
        {
            if (this.TagLookup.TryGetComponent(caster, out var statName))
            {
                statNames.Add((FixedString64Bytes)statName);
            }
        }
    }
}