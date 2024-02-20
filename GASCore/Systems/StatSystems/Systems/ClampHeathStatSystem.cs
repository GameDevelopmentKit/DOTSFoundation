namespace GASCore.Systems.StatSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.StatSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityLogicEffectGroup))]
    [UpdateAfter(typeof(AggregateStatModifierSystem))]
    [UpdateBefore(typeof(ApplyInstantStatModifierSystem))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct ClampHeathStatSystem : ISystem
    {
        private StatAspect.Lookup statAspectLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state) { this.statAspectLookup = new StatAspect.Lookup(ref state); }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            this.statAspectLookup.Update(ref state);
            new ClampHeathStatJob()
            {
                StatAspectLookup = this.statAspectLookup
            }.ScheduleParallel();
        }
    }

    [BurstCompile]
    [WithAll(typeof(HealingTag))]
    [WithChangeFilter(typeof(ModifierAggregatorData))]
    public partial struct ClampHeathStatJob : IJobEntity
    {
        [ReadOnly] public StatAspect.Lookup StatAspectLookup;
        public void Execute(ref DynamicBuffer<ModifierAggregatorData> modifierAggregatorBuffer, in CasterComponent caster, in AffectedTargetComponent affectedTarget)
        {
            if (!this.StatAspectLookup.TryGetAspect(affectedTarget, out var targetStatAspect) || !targetStatAspect.HasStat(StatName.MaxHealth)) return;

            for (var index = 0; index < modifierAggregatorBuffer.Length; index++)
            {
                var aggregatorData = modifierAggregatorBuffer[index];
                if (!aggregatorData.TargetStat.Equals(StatName.Health)) continue;

                var newHealthValue = targetStatAspect.CalculateStatValue(aggregatorData);
                var maxHealth      = targetStatAspect.GetCurrentValue(StatName.MaxHealth);

                if (newHealthValue > maxHealth)
                {
                    var addedHealthValue = maxHealth - targetStatAspect.GetCurrentValue(StatName.Health);
                    // modify health stat
                    modifierAggregatorBuffer.Add(new ModifierAggregatorData(StatName.Health, addedHealthValue));
                    modifierAggregatorBuffer.RemoveAtSwapBack(index);
                }
                
                break;
            }
        }
    }
}