namespace GASCore.Systems.StatSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.LogicEffectSystems.Components;
    using GASCore.Systems.StatSystems.Components;
    using Unity.Burst;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityCleanupSystemGroup), OrderFirst = true)]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct ApplyPeriodEffectPolicySystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new ApplyPeriodEffectPolicyJob().ScheduleParallel(); 
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
        void Execute(EnabledRefRW<EndTimeComponent> enabledRefRW) { enabledRefRW.ValueRW = true; }
    }
}