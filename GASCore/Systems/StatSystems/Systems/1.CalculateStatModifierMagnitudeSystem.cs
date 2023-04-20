namespace GASCore.Systems.StatSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using GASCore.Systems.StatSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [UpdateInGroup(typeof(AbilityLogicEffectGroup), OrderFirst = true)]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct CalculateStatModifierMagnitudeSystem : ISystem
    {
        private StatAspect.Lookup statAspectLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state) { this.statAspectLookup = new StatAspect.Lookup(ref state, true); }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            this.statAspectLookup.Update(ref state);
            var random = new Random((uint)(SystemAPI.Time.ElapsedTime * 100000));

            state.Dependency = new CalculateScalableFloatMagnitudeJob().ScheduleParallel(state.Dependency);

            state.Dependency = new CalculateRandomIntInRangeMagnitudeJob()
            {
                Random = random,
            }.ScheduleParallel(state.Dependency);

            state.Dependency = new CalculateStatBasedMagnitudeJob()
            {
                CasterLookup         = SystemAPI.GetComponentLookup<CasterComponent>(true),
                AffectedTargetLookup = SystemAPI.GetComponentLookup<AffectedTargetComponent>(true),
                StatAspectLookup     = this.statAspectLookup,
            }.ScheduleParallel(state.Dependency);
        }
    }

    [WithChangeFilter(typeof(ScalableFloatMagnitudeCalculation))]
    [WithNone(typeof(PeriodEffectInstanceTag))]
    [BurstCompile]
    public partial struct CalculateScalableFloatMagnitudeJob : IJobEntity
    {
        private void Execute(in ScalableFloatMagnitudeCalculation scalableFloat, ref StatModifierData statModifierData) { statModifierData.ModifierMagnitude = scalableFloat.Value; }
    }

    [WithChangeFilter(typeof(RandomIntInRangeMagnitudeCalculation))]
    [WithNone(typeof(PeriodEffectInstanceTag))]
    [BurstCompile]
    public partial struct CalculateRandomIntInRangeMagnitudeJob : IJobEntity
    {
        public Random Random;

        private void Execute(in RandomIntInRangeMagnitudeCalculation range, ref StatModifierData statModifierData) { statModifierData.ModifierMagnitude = this.Random.NextInt(range.Min, range.Max); }
    }

    [WithChangeFilter(typeof(StatBasedMagnitudeCalculation))]
    [WithNone(typeof(PeriodEffectInstanceTag))]
    [BurstCompile]
    public partial struct CalculateStatBasedMagnitudeJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<CasterComponent>         CasterLookup;
        [ReadOnly] public ComponentLookup<AffectedTargetComponent> AffectedTargetLookup;
        [ReadOnly] public StatAspect.Lookup                        StatAspectLookup;

        private void Execute(ref StatModifierData statModifierData, in StatBasedMagnitudeCalculation attributeBased, in Parent effectEntity)
        {
            var sourceEntity = attributeBased.SourceType switch
            {
                SourceType.Caster => this.CasterLookup[effectEntity.Value].Value,
                SourceType.AffectedTarget => this.AffectedTargetLookup[effectEntity.Value].Value,
                _ => default
            };
            var statAspect = this.StatAspectLookup[sourceEntity];

            statModifierData.ModifierMagnitude = statAspect.HasStat(attributeBased.SourceStat)
                ? attributeBased.Coefficient * statAspect.GetCurrentValue(attributeBased.SourceStat)
                : attributeBased.Coefficient;
        }
    }
}