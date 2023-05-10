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
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<FindTargetComponent>();
            state.RequireForUpdate(SystemAPI.QueryBuilder().WithAny<FilterIncludeStatName, FilterKillCounter>().Build());
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            var convertKillCounterJob = new ConvertFilterKillCounterToFilterIncludeStatNameJob
            {
                Ecb            = ecb,
                FilterLookup   = SystemAPI.GetBufferLookup<FilterIncludeStatName>(),
                StatNameLookup = SystemAPI.GetComponentLookup<UpdateKillCountStatNameComponent>(true),
            }.ScheduleParallel(state.Dependency);

            state.Dependency = new FilterIncludeStatNameJob
            {
                StatNamesLookup = SystemAPI.GetComponentLookup<StatNameToIndex>(true),
            }.ScheduleParallel(convertKillCounterJob);
        }
    }

    [WithChangeFilter(typeof(FilterKillCounter))]
    [BurstCompile]
    public partial struct ConvertFilterKillCounterToFilterIncludeStatNameJob : IJobEntity
    {
        public            EntityCommandBuffer.ParallelWriter                Ecb;
        [ReadOnly] public BufferLookup<FilterIncludeStatName>               FilterLookup;
        [ReadOnly] public ComponentLookup<UpdateKillCountStatNameComponent> StatNameLookup;

        private void Execute(
            Entity entity,
            [EntityIndexInQuery] int index,
            in CasterComponent caster
        )
        {
            if (!this.FilterLookup.HasBuffer(entity))
            {
                this.Ecb.AddBuffer<FilterIncludeStatName>(index, entity);
            }

            this.Ecb.AppendToBuffer(index, entity, new FilterIncludeStatName { Value = this.StatNameLookup[caster] });
        }
    }

    [WithAll(typeof(FindTargetComponent))]
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
                var targetStatNames = this.StatNamesLookup[targetables[i]].Value;
                var isValid = true;
                foreach (var statName in statNames)
                {
                    if (!targetStatNames.ContainsKey(statName))
                    {
                        isValid = false;
                        break;
                    }
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
}