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

        protected override void OnUpdate()
        {
            var scalableFloatMagnitudeJob = new CalculateScalableFloatMagnitudeJob().ScheduleParallel(Dependency);

            var attributeBasedMagnitudeJob = Entities.WithBurst().WithChangeFilter<StatBasedMagnitudeCalculation>().WithNone<PeriodEffectInstanceTag>().ForEach(
                (ref StatModifierData statModifierData, in StatBasedMagnitudeCalculation attributeBased, in Parent effectEntity) =>
                {
                    Entity sourceEntity = default;
                    if (attributeBased.SourceType == SourceType.AffectedTarget)
                    {
                        sourceEntity = GetComponent<AffectedTargetComponent>(effectEntity.Value).Value;
                    }
                    else if (attributeBased.SourceType == SourceType.Caster)
                    {
                        sourceEntity = GetComponent<CasterComponent>(effectEntity.Value).Value;
                    }

                    var statAspect = SystemAPI.GetAspectRO<StatAspect>(sourceEntity);
                    // Debug.Log($"attributeBasedMagnitudeJob - source entity is {attributeBased.SourceType} - has {attributeBased.SourceStat} : {statAspect.HasStat(attributeBased.SourceStat)}");

                    if (statAspect.HasStat(attributeBased.SourceStat))
                    {
                        statModifierData.ModifierMagnitude = attributeBased.Coefficient * statAspect.GetCurrentValue(attributeBased.SourceStat);
                    }
                    else
                    {
                        statModifierData.ModifierMagnitude = attributeBased.Coefficient;
                    }

                    // Debug.Log($"attributeBasedMagnitudeJob magnitude = {statModifierData.ModifierMagnitude}");
                }).ScheduleParallel(scalableFloatMagnitudeJob);

            Dependency = attributeBasedMagnitudeJob;
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