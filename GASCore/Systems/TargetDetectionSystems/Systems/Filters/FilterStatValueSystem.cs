namespace GASCore.Systems.TargetDetectionSystems.Systems.Filters
{
    using GASCore.Systems.StatSystems.Components;
    using GASCore.Systems.TargetDetectionSystems.Components;
    using GASCore.Systems.TargetDetectionSystems.Components.Filters;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;

    [UpdateInGroup(typeof(FilterTargetGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct FilterStatValueSystem : ISystem
    {
        private StatAspect.Lookup statAspectLookup;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<FindTargetComponent>();
            this.statAspectLookup = new StatAspect.Lookup(ref state);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            this.statAspectLookup.Update(ref state);
            new FilterStatValueJob { StatAspectLookup = this.statAspectLookup }.ScheduleParallel();
        }
    }


    [WithAll(typeof(FindTargetComponent))]
    [WithAll(typeof(FilterStatValue))]
    [BurstCompile]
    public partial struct FilterStatValueJob : IJobEntity
    {
        [ReadOnly] public StatAspect.Lookup StatAspectLookup;

        private void Execute(ref DynamicBuffer<TargetableElement> targetables, in FilterStatValue filterStat)
        {
            for (var i = 0; i < targetables.Length;)
            {
                if (this.StatAspectLookup.TryGetAspect(targetables[i], out var targetStatAspect))
                {
                    var statData = targetStatAspect.GetStatData(filterStat.StatName);
                    if (statData.HasValue)
                    {
                        var currentValue = filterStat.Percent
                            ? statData.Value.CurrentValue / statData.Value.OriginValue
                            : statData.Value.CurrentValue;
                        if (filterStat.Above && currentValue >= filterStat.Value || !filterStat.Above && currentValue <= filterStat.Value)
                        {
                            // match 
                            ++i;
                            continue;
                        }
                    }
                }

                // no matching stat
                targetables.RemoveAtSwapBack(i);
            }
        }
    }
}