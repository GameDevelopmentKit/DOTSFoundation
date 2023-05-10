namespace GASCore.Systems.StatSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using GASCore.Systems.StatSystems.Components;
    using GASCore.Systems.TimelineSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityLogicEffectGroup))]
    [UpdateAfter(typeof(ApplyDurationEffectPolicySystem))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct ApplyInstantStatModifierSystem : ISystem
    {
        private EntityQuery instantEffectEntityQuery;
        private EntityQuery periodEffectEntityQuery;

        private StatAspect.Lookup statAspectLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            this.statAspectLookup = new StatAspect.Lookup(ref state, false);
            using var queryBuilder = new EntityQueryBuilder(Allocator.Temp).WithAll<InstantEffect, ModifierAggregatorData, AffectedTargetComponent>();
            this.instantEffectEntityQuery = state.GetEntityQuery(queryBuilder);

            queryBuilder.Reset();
            // queryBuilder.WithAll<ModifierAggregatorData, AffectedTargetComponent>().WithAny<DurationEffect, InfiniteEffect>().WithNone<PeriodEffect>();
            queryBuilder.WithAll<PeriodEffect, ModifierAggregatorData, AffectedTargetComponent>().WithAny<DurationEffect, InfiniteEffect>().WithNone<EndTimeComponent>();
            this.periodEffectEntityQuery = state.GetEntityQuery(queryBuilder);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            this.statAspectLookup.Update(ref state);

            var applyInstantEffectJob = new ApplyInstantStatModifierJob()
            {
                StatAspectLookup = this.statAspectLookup
            }.Schedule(this.instantEffectEntityQuery, state.Dependency);

            state.Dependency = new ApplyInstantStatModifierJob()
            {
                StatAspectLookup = this.statAspectLookup
            }.Schedule(this.periodEffectEntityQuery, applyInstantEffectJob);
        }
    }

    [BurstCompile]
    public partial struct ApplyInstantStatModifierJob : IJobEntity
    {
        public StatAspect.Lookup StatAspectLookup;
        void Execute(in AffectedTargetComponent affectedTarget, in DynamicBuffer<ModifierAggregatorData> modifierAggregatorBuffer)
        {
            var statAspect = this.StatAspectLookup[affectedTarget.Value];
            foreach (var aggregator in modifierAggregatorBuffer)
            {
                statAspect.SetBaseValue(aggregator.TargetStat, statAspect.CalculateStatValue(aggregator));
            }
        }
    }
}
