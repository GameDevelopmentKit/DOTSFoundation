namespace GASCore.Systems.StatSystems.Systems
{
    using System;
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.StatSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Unity.Entities;
    using Random = Unity.Mathematics.Random;

    [UpdateInGroup(typeof(AbilityLogicEffectGroup))]
    [UpdateAfter(typeof(AggregateStatModifierSystem))]
    [UpdateBefore(typeof(ApplyInstantStatModifierSystem))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct DealDamageSystem : ISystem
    {
        private StatAspect.Lookup statAspectLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state) { this.statAspectLookup = new StatAspect.Lookup(ref state); }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            this.statAspectLookup.Update(ref state);
            new DealDamageJob()
            {
                StatAspectLookup = this.statAspectLookup
            }.ScheduleParallel();
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

                var damage = 0f;
                // If caster has stat data
                if (this.TryGetAspect(this.StatAspectLookup, caster, out var casterStatAspect))
                {
                    // Calculate base damage from aggregator
                    damage = casterStatAspect.CalculateStatValue(aggregatorData);
                    // critical or not
                    var random = Random.CreateFromIndex((uint)entityInQueryIndex);
                    if (random.NextFloat() < casterStatAspect.GetCurrentValue(StatName.CriticalStrikeChance))
                    {
                        damage += damage * casterStatAspect.GetCurrentValue(StatName.CriticalStrikeDamage);
                    }
                }
                else
                {
                    damage = aggregatorData.Add * aggregatorData.Multiply / aggregatorData.Divide;
                }

                var affectedTargetStatAspect = this.StatAspectLookup[affectedTarget.Value];
                //add caster armor
                damage = Math.Max(damage - affectedTargetStatAspect.GetCurrentValue(StatName.Armor), 0);
                var currentHealth = affectedTargetStatAspect.GetCurrentValue(StatName.Health);
                if (currentHealth - damage < 0)
                    damage = currentHealth;

                // modify health stat
                modifierAggregatorBuffer.Add(new ModifierAggregatorData(StatName.Health, -damage));
                modifierAggregatorBuffer.RemoveAtSwapBack(index);
                break;
            }
        }

        public bool TryGetAspect(StatAspect.Lookup statAspectLookup, Entity entity, out StatAspect statAspect)
        {
            if (!Has(statAspectLookup, entity))
            {
                statAspect = default;
                return false;
            }
        
            statAspect = statAspectLookup[entity];
            return true;
        }
        
        public bool Has(StatAspect.Lookup statAspectLookup, Entity entity)
        {
            ref var sa = ref UnsafeUtility.As<StatAspect.Lookup, StatAspectAsLookup>(ref statAspectLookup);
            return sa.StatDataBufferLookup.HasBuffer(entity) && sa.StatNameToIndexLookup.HasComponent(entity);
        }
        
        private struct StatAspectAsLookup
        {
            public BufferLookup<StatDataElement>    StatDataBufferLookup;
            public ComponentLookup<StatNameToIndex> StatNameToIndexLookup;
        }
    }
}