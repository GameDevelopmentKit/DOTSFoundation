namespace GASCore.Systems.StatSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.StatSystems.Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Jobs;
    using Unity.Transforms;
    using UnityEngine;

    [UpdateInGroup(typeof(AbilityLogicEffectGroup), OrderFirst = true)]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial class CalculateStatModifierMagnitudeSystem : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem endSimEcbSystem;

        protected override void OnCreate() { this.endSimEcbSystem = this.World.GetExistingSystemManaged<EndSimulationEntityCommandBufferSystem>(); }

        protected override void OnUpdate()
        {
            var scalableFloatMagnitudeJob = new CalculateScalableFloatMagnitudeJob().ScheduleParallel(Dependency);

            var attributeBasedMagnitudeJob = Entities.WithBurst().WithChangeFilter<AttributeBasedMagnitudeCalculation>().WithNone<PeriodEffectInstanceTag>().ForEach(
                (ref StatModifierData statModifierData, in AttributeBasedMagnitudeCalculation attributeBased, in Parent effectEntity) =>
                {
                    Entity sourceEntity = default;
                    if (attributeBased.SourceType == SourceType.Target)
                    {
                        sourceEntity = GetComponent<AffectedTargetComponent>(effectEntity.Value).Value;
                    }
                    else if (attributeBased.SourceType == SourceType.Source)
                    {
                        sourceEntity = GetComponent<CasterComponent>(effectEntity.Value).Value;
                    }

                    var statAspect = SystemAPI.GetAspectRO<StatAspect>(sourceEntity);
                    if (statAspect.HasStat(attributeBased.SourceAttribute))
                    {
                        statModifierData.ModifierMagnitude = attributeBased.Coefficient * statAspect.GetCurrentValue(attributeBased.SourceAttribute);
                    }
                    else
                    {
                        statModifierData.ModifierMagnitude = attributeBased.Coefficient;
                    }

                    Debug.Log($"attributeBasedMagnitudeJob magnitude = {statModifierData.ModifierMagnitude}");
                }).ScheduleParallel(Dependency);

            Dependency = JobHandle.CombineDependencies(scalableFloatMagnitudeJob, attributeBasedMagnitudeJob);

            this.endSimEcbSystem.AddJobHandleForProducer(Dependency);
        }
    }

    [BurstCompile]
    [WithChangeFilter(typeof(ScalableFloatMagnitudeCalculation))]
    [WithNone(typeof(PeriodEffectInstanceTag))]
    public partial struct CalculateScalableFloatMagnitudeJob : IJobEntity
    {
        void Execute(in ScalableFloatMagnitudeCalculation scalableFloat, ref StatModifierData statModifierData)
        {
            statModifierData.ModifierMagnitude = scalableFloat.Value;
            // Debug.Log($"CalculateScalableFloatMagnitudeJob magnitude = {statModifierData.ModifierMagnitude}");
        }
    }
}