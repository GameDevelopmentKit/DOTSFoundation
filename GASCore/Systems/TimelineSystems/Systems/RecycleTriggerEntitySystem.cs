namespace GASCore.Systems.TimelineSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.TimelineSystems.Components;
    using Unity.Burst;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityCleanupSystemGroup), OrderFirst = true)]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct RecycleTriggerEntitySystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state) { new RecycleTriggerEntityJob().ScheduleParallel(); }
    }

    [BurstCompile]
    [WithAll(typeof(RecycleTriggerEntityTag), typeof(CompletedAllTriggerConditionTag))]
    [WithDisabled(typeof(InTriggerConditionResolveProcessTag))]
    public partial struct RecycleTriggerEntityJob : IJobEntity
    {
        void Execute(ref DynamicBuffer<CompletedTriggerElement> completedTriggerBuffer,
            EnabledRefRW<CompletedAllTriggerConditionTag> completedAllTriggerConditionEnableState,
            EnabledRefRW<InTriggerConditionResolveProcessTag> inTriggerConditionResolveProcessEnableState)
        {
            completedAllTriggerConditionEnableState.ValueRW     = false;
            inTriggerConditionResolveProcessEnableState.ValueRW = true;
            completedTriggerBuffer.Clear();
        }
    }
}