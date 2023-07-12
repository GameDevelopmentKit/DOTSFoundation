namespace GASCore.Systems.StatSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.LogicEffectSystems.Components;
    using GASCore.Systems.StatSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityCleanupSystemGroup), OrderFirst = true)]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct ApplyPeriodEffectPolicySystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new ApplyPeriodEffectPolicyJob()
            {
                CurrentElapsedTime = SystemAPI.Time.ElapsedTime,
                EndTimeLookup      = SystemAPI.GetComponentLookup<EndTimeComponent>()
            }.ScheduleParallel();
        }
    }

    /// <summary>
    /// Add end time for period effect
    /// </summary>
    [BurstCompile]
    [WithAll(typeof(PeriodEffect))]
    [WithDisabled(typeof(EndTimeComponent))]
    public partial struct ApplyPeriodEffectPolicyJob : IJobEntity
    {
        public                                       double                            CurrentElapsedTime;
        [NativeDisableParallelForRestriction] public ComponentLookup<EndTimeComponent> EndTimeLookup;

        void Execute(Entity statModifierEntity)
        {
            //wait period in second
            var endTimeComponent = this.EndTimeLookup[statModifierEntity];
            endTimeComponent.NextEndTimeValue      = this.CurrentElapsedTime + endTimeComponent.AmountTime;
            this.EndTimeLookup[statModifierEntity] = endTimeComponent;
            this.EndTimeLookup.SetComponentEnabled(statModifierEntity, true);
        }
    }
}