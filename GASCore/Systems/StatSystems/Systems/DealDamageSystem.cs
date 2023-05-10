namespace GASCore.Systems.StatSystems.Systems
{
    using System;
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.StatSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Random = Unity.Mathematics.Random;

    [UpdateInGroup(typeof(AbilityLogicEffectGroup))]
    [UpdateAfter(typeof(AggregateStatModifierSystem))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct DealDamageSystem : ISystem
    {
        private StatAspect.Lookup statAspectLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state) { this.statAspectLookup = new StatAspect.Lookup(ref state, true); }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            this.statAspectLookup.Update(ref state);
            new DealDamageJob() { StatAspectLookup = this.statAspectLookup }.ScheduleParallel();
        }
    }

    [BurstCompile]
    [WithAll(typeof(DealDamageTag))]
    [WithChangeFilter(typeof(ModifierAggregatorData))]
    public partial struct DealDamageJob : IJobEntity
    {
        [ReadOnly] public StatAspect.Lookup StatAspectLookup;
        public void Execute([EntityIndexInQuery] int entityInQueryIndex, ref DynamicBuffer<ModifierAggregatorData> modifierAggregatorBuffer, in CasterComponent caster,
            in AffectedTargetComponent affectedTarget)
        {
            for (var index = 0; index < modifierAggregatorBuffer.Length; index++)
            {
                var aggregatorData = modifierAggregatorBuffer[index];
                if (!aggregatorData.TargetStat.Equals(StatName.Damage)) continue;
                var casterStatAspect         = this.StatAspectLookup[caster.Value];
                var affectedTargetStatAspect = this.StatAspectLookup[affectedTarget.Value];

                // Calculate base damage from aggregator
                var damage = casterStatAspect.CalculateStatValue(aggregatorData);
                // critical or not
                var random = Random.CreateFromIndex((uint)entityInQueryIndex);
                if (random.NextFloat() < casterStatAspect.GetCurrentValue(StatName.CriticalStrikeChance))
                {
                    damage += damage * casterStatAspect.GetCurrentValue(StatName.CriticalStrikeDamage);
                }

                //add caster armor
                damage = Math.Max(damage - affectedTargetStatAspect.GetCurrentValue(StatName.Armor), 0);
                var currentHealth = affectedTargetStatAspect.GetCurrentValue(StatName.Health);
                if (currentHealth - damage < 0)
                    damage = currentHealth;

                // modify health stat
                modifierAggregatorBuffer.Add( new ModifierAggregatorData(StatName.Health,-damage));
                modifierAggregatorBuffer.RemoveAtSwapBack(index);
                break;
            }
        }
    }
}