namespace GASCore.Systems.LogicEffectSystems.Systems
{
    using GASCore.Blueprints;
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.CommonSystems.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using GASCore.Systems.StatSystems.Components;
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
        private ComponentLookup<TeamOwnerId>     teamLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            using var queryBuilder = new EntityQueryBuilder(Allocator.Temp).WithAll<TeamOwnerId>().WithAll<StatNameToIndex>();
            this.entityQuery           = state.GetEntityQuery(queryBuilder);
            this.statNameToIndexLookup = state.GetComponentLookup<StatNameToIndex>(true);
            this.teamLookup            = state.GetComponentLookup<TeamOwnerId>(true);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            // var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            var entities     = this.entityQuery.ToEntityListAsync(state.WorldUpdateAllocator, state.Dependency, out var queryJob);
            this.statNameToIndexLookup.Update(ref state);
            this.teamLookup.Update(ref state);

            state.Dependency = new FindTargetWithStatsJob()
            {
                Entities              = entities,
                StatNameToIndexLookup = this.statNameToIndexLookup,
                TeamLookup            = this.teamLookup,
            }.ScheduleParallel(queryJob);
        }
    }

    [WithChangeFilter(typeof(TargetWithStatElement))]
    [BurstCompile]
    public partial struct FindTargetWithStatsJob : IJobEntity
    {
        [ReadOnly] public NativeList<Entity>               Entities;
        [ReadOnly] public ComponentLookup<StatNameToIndex> StatNameToIndexLookup;
        [ReadOnly] public ComponentLookup<TeamOwnerId>     TeamLookup;

        private void Execute(
            in CasterComponent caster,
            in DynamicBuffer<TargetWithStatElement> statNames,
            in DynamicBuffer<TargetTypeElement> targetTypes,
            ref DynamicBuffer<TargetElement> targets,
            ref TriggerConditionCount triggerConditionCount
        )
        {
            var casterTeam = this.TeamLookup[caster.Value].Value;
            foreach (var target in this.Entities)
            {
                var statNameToIndex = this.StatNameToIndexLookup[target].BlobValue.Value;
                var targetTeam      = this.TeamLookup[target].Value;
                var isTarget        = false;

                foreach (var targetType in targetTypes)
                {
                    switch (targetType.Value)
                    {
                        case TargetType.Opponent:
                            if (casterTeam != targetTeam) isTarget = true;
                            break;
                        case TargetType.Self:
                            if (caster.Value == target) isTarget = true;
                            break;
                        case TargetType.Ally:
                            if (casterTeam == targetTeam && caster.Value != target) isTarget = true;
                            break;
                    }

                    if (isTarget) break;
                }

                if (!isTarget) continue; // wrong target type

                foreach (var statName in statNames)
                {
                    if (!statNameToIndex.ContainsKey(statName.Value))
                    {
                        isTarget = false;
                        break;
                    }
                }

                if (!isTarget) continue; // wrong stat name

                targets.Add(new TargetElement() { Value = target });
            }

            // count as complete even if no target found
            --triggerConditionCount.Value;
        }
    }
}