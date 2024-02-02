namespace GASCore.Systems.TargetDetectionSystems.Systems.Filters
{
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.StatSystems.Components;
    using GASCore.Systems.TargetDetectionSystems.Components;
    using GASCore.Systems.TargetDetectionSystems.Components.Filters;
    using GASCore.Systems.TimelineSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;

    [UpdateInGroup(typeof(FilterTargetGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct FilterIncludeStatNameSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { state.RequireForUpdate<FindTargetTagComponent>(); }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new ConvertFilterKillCounterToFilterIncludeStatNameJob
            {
                StatNameLookup = SystemAPI.GetComponentLookup<UpdateKillCountStatNameComponent>(true),
            }.ScheduleParallel();
            new FilterIncludeStatNameJob { StatNamesLookup = SystemAPI.GetComponentLookup<StatNameToIndex>(true), }.ScheduleParallel();
        }
    }


    [WithAll(typeof(FindTargetTagComponent))]
    [WithAll(typeof(FilterIncludeStatName))]
    [BurstCompile]
    public partial struct FilterIncludeStatNameJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<StatNameToIndex> StatNamesLookup;

        private void Execute(
            ref DynamicBuffer<TargetableElement> targetables,
            in DynamicBuffer<FilterIncludeStatName> statNames
        )
        {
            for (var i = 0; i < targetables.Length;)
            {
                var isValid = true;

                if (this.StatNamesLookup.HasComponent(targetables[i]))
                {
                    var targetStatNames = this.StatNamesLookup[targetables[i]].Value;
                    foreach (var statName in statNames)
                    {
                        if (!targetStatNames.ContainsKey(statName))
                        {
                            isValid = false;
                            break;
                        }
                    }
                }
                else
                {
                    isValid = false;
                }
              

                if (!isValid)
                {
                    // no matching stat
                    targetables.RemoveAtSwapBack(i);
                    continue;
                }

                ++i;
            }
        }
    }

    [WithAll(typeof(FilterKillCounter))]
    [BurstCompile]
    public partial struct ConvertFilterKillCounterToFilterIncludeStatNameJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<UpdateKillCountStatNameComponent> StatNameLookup;

        private void Execute(in CasterComponent caster, ref DynamicBuffer<FilterIncludeStatName> filterIncludeStatNameBuffer)
        {
            filterIncludeStatNameBuffer.Add(new FilterIncludeStatName { Value = this.StatNameLookup[caster] });
        }
    }
}