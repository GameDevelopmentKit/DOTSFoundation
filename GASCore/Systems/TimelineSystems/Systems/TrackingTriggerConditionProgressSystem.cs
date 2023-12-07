namespace GASCore.Systems.TimelineSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.TimelineSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityMainFlowGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct TrackingTriggerConditionProgressSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state) { state.Dependency = new TrackingTriggerConditionProgressJob().ScheduleParallel(state.Dependency); }
    }
    
    [WithChangeFilter(typeof(CompletedTriggerElement))]
    [WithDisabled(typeof(CompletedAllTriggerConditionTag))]
    [BurstCompile]
    public partial struct TrackingTriggerConditionProgressJob : IJobEntity
    {
        void Execute(ref DynamicBuffer<CompletedTriggerElement> completedTriggerBuffer, in TriggerConditionAmount conditionAmount,
            EnabledRefRW<CompletedAllTriggerConditionTag> completedAllTriggerConditionEnableState,
            EnabledRefRW<InTriggerConditionResolveProcessTag> inTriggerConditionResolveProcessEnableState)
        {
            if (conditionAmount.Value != 0 && (conditionAmount.Value != 1 || completedTriggerBuffer.Length != 1))
            {
                var conditionHashset = new NativeHashSet<int>(conditionAmount.Value, Allocator.Temp);

                for (var index = 0; index < completedTriggerBuffer.Length;)
                {
                    var completedTriggerIndex = completedTriggerBuffer[index];
                    if (!conditionHashset.Add(completedTriggerIndex.Index))
                    {
                        completedTriggerBuffer.RemoveAtSwapBack(index);
                    }
                    else
                    {
                        index++;
                    }
                }

                if (conditionHashset.Count != conditionAmount.Value) return;
            }

            completedAllTriggerConditionEnableState.ValueRW     = true;
            inTriggerConditionResolveProcessEnableState.ValueRW = false;
        }
    }
}