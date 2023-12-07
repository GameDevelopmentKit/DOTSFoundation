namespace GASCore.Systems.AbilityMainFlow.Systems.AbilitySystem
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using Unity.Burst;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityMainFlowGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct ValidateRequestActiveAbilitySystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var job = new ValidateAbilityJob().ScheduleParallel(state.Dependency);
            job.Complete();
        }
    }

    [WithAll(typeof(RequestActivate), typeof(AbilityId))]
    [WithNone(typeof(Duration))]
    [WithDisabled(typeof(GrantedActivation))]
    public partial struct ValidateAbilityJob : IJobEntity
    {
        private void Execute(EnabledRefRW<RequestActivate> requestActivateEnableState,
            EnabledRefRW<GrantedActivation> grantedActivationEnableState)
        {
            requestActivateEnableState.ValueRW   = false;
            grantedActivationEnableState.ValueRW = true;
        }
    }
}