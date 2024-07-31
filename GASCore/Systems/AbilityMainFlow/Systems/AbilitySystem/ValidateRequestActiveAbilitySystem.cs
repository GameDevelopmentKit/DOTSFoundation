namespace GASCore.Systems.AbilityMainFlow.Systems.AbilitySystem
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using GASCore.Systems.StatSystems.Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Collections;
    using UnityEngine;

    [UpdateInGroup(typeof(AbilityMainFlowGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct ValidateRequestActiveAbilitySystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var job = new ValidateAbilityJob() { statDataLookup = state.GetBufferLookup<StatDataElement>() }.ScheduleParallel(state.Dependency);
            job.Complete();
        }
    }

    [WithAll(typeof(RequestActivate), typeof(Components.AbilityId))]
    [WithNone(typeof(Duration))]
    [WithDisabled(typeof(GrantedActivation))]
    public partial struct ValidateAbilityJob : IJobEntity
    {
        [ReadOnly] public BufferLookup<StatDataElement> statDataLookup;
        private void Execute(EnabledRefRW<RequestActivate> requestActivateEnableState,
            EnabledRefRW<GrantedActivation> grantedActivationEnableState, CasterComponent caster, DynamicBuffer<AbilityCost> abilityCosts)
        {

            // Check if the caster has enough resources to cast the ability
            if (abilityCosts.Length > 0)
            {
                DynamicBuffer<StatDataElement> casterStatData = this.statDataLookup[caster.Value];
                for (int i = 0; i < abilityCosts.Length; i++)
                {
                    for (int j = 0; j < casterStatData.Length; j++)
                    {
                        if (casterStatData[j].StatName == abilityCosts[i].Name)
                        {
                            // If current caster's stat does not have enough resources to cast the ability, don't grantActivation to ability
                            if (casterStatData[j].BaseValue < abilityCosts[i].Value)
                            {
                                return;
                            }
                        }
                    }
                }
            }


            requestActivateEnableState.ValueRW = false;
            grantedActivationEnableState.ValueRW = true;
        }
    }
}