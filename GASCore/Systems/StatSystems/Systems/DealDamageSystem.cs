namespace GASCore.Systems.StatSystems.Systems
{
    using System;
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.StatSystems.Components;
    using Unity.Burst;
    using Unity.Entities;
    using Random = Unity.Mathematics.Random;

    [UpdateInGroup(typeof(AbilityLogicEffectGroup))]
    [UpdateAfter(typeof(AggregateStatModifierSystem))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial class DealDamageSystem : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem endSimEcbSystem;

        protected override void OnCreate() { this.endSimEcbSystem = this.World.GetExistingSystemManaged<EndSimulationEntityCommandBufferSystem>(); }

        protected override void OnUpdate()
        {
            var ecb = this.endSimEcbSystem.CreateCommandBuffer().AsParallelWriter();

            this.Dependency = Entities.WithAll<DealDamageTag>().WithChangeFilter<ModifierAggregatorData>()
                .ForEach((Entity effectEntity, int entityInQueryIndex, ref DynamicBuffer<ModifierAggregatorData> modifierAggregatorBuffer, in CasterComponent caster,
                    in AffectedTargetComponent affectedTarget) =>
                {
                    for (var index = 0; index < modifierAggregatorBuffer.Length; index++)
                    {
                        var aggregatorData = modifierAggregatorBuffer[index];
                        if (!aggregatorData.TargetStat.Equals(StatName.Damage)) continue;
                        var casterStatAspect         = SystemAPI.GetAspectRO<StatAspect>(caster.Value);
                        var affectedTargetStatAspect = SystemAPI.GetAspectRO<StatAspect>(affectedTarget.Value);

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

                        // modify health stat
                        modifierAggregatorBuffer.Add(new ModifierAggregatorData()
                        {
                            TargetStat = StatName.Health,
                            Add        = -damage,
                            Multiply   = 1,
                            Divide   = 1
                        });
                        
                        modifierAggregatorBuffer.RemoveAtSwapBack(index);
                        break;
                    }
                }).ScheduleParallel(this.Dependency);
            this.endSimEcbSystem.AddJobHandleForProducer(this.Dependency);
        }
    }
}