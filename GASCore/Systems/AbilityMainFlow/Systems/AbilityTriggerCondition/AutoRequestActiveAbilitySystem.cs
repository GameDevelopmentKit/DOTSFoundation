namespace GASCore.Systems.AbilityMainFlow.Systems.AbilityTriggerCondition
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using GASCore.Systems.TimelineSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityMainFlowGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct AutoRequestActiveAbilitySystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new AutoTriggerAbilityAfterCooldownJob().ScheduleParallel();
            new AutoTriggerAbilityOnStartJob().ScheduleParallel();

            new AutoRequestActiveAbilityJob()
            {
                RecycleTriggerLookup = SystemAPI.GetComponentLookup<RecycleTriggerEntityTag>(true)
            }.ScheduleParallel();
        }
        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }

    [BurstCompile]
    [WithAll(typeof(AbilityId), typeof(CompletedAllTriggerConditionTag), typeof(AutoActiveTag))]
    [WithDisabled(typeof(RequestActivate))]
    public partial struct AutoRequestActiveAbilityJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<RecycleTriggerEntityTag> RecycleTriggerLookup;
        void Execute(Entity abilityEntity, EnabledRefRW<AutoActiveTag> autoActiveEnableState, EnabledRefRW<RequestActivate> requestActivateEnableState)
        {
            // Debug.Log($"AutoRequestActiveAbilityJob ability {abilityId.Value}");
            if (!this.RecycleTriggerLookup.HasComponent(abilityEntity))
            {
                autoActiveEnableState.ValueRW = false;
            }

            requestActivateEnableState.ValueRW = true;
        }
    }

    [BurstCompile]
    [WithAll(typeof(AbilityId), typeof(Cooldown), typeof(AutoActiveAfterCooldownTag))]
    [WithNone(typeof(Duration))]
    public partial struct AutoTriggerAbilityAfterCooldownJob : IJobEntity
    {
        void Execute(ref DynamicBuffer<CompletedTriggerElement> completedTriggerBuffer)
        {
            completedTriggerBuffer.Add(new CompletedTriggerElement() { Index = TypeManager.GetTypeIndex<AutoActiveAfterCooldownTag>() });
        }
    }

    [BurstCompile]
    [WithAll(typeof(AbilityId), typeof(AutoActiveOnStartTag))]
    [WithChangeFilter(typeof(AutoActiveOnStartTag))]
    public partial struct AutoTriggerAbilityOnStartJob : IJobEntity
    {
        void Execute(ref DynamicBuffer<CompletedTriggerElement> completedTriggerBuffer)
        {
            completedTriggerBuffer.Add(new CompletedTriggerElement() { Index = TypeManager.GetTypeIndex<AutoActiveOnStartTag>() });
        }
    }
}