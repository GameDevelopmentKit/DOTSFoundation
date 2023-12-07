namespace GASCore.Systems.StatSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using GASCore.Systems.StatSystems.Components;
    using GASCore.Systems.TimelineSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    
    [UpdateInGroup(typeof(AbilityLogicEffectGroup))]
    [UpdateAfter(typeof(CalculateAddedStatValueSystem))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct NotifyStatChangeEventSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state) { new NotifyStatChangedJob().ScheduleParallel(); }
    }
    
    [BurstCompile]
    [WithChangeFilter(typeof(StatDataElement))]
    [WithDisabled(typeof(OnStatChangeTag))]
    public partial struct NotifyStatChangedJob : IJobEntity
    {
        void Execute(ref DynamicBuffer<StatDataElement> statDataBuffer, ref DynamicBuffer<StatChangeElement> onStatChangeBuffer, EnabledRefRW<OnStatChangeTag> statChangeEnableState)
        {
            var hasStatChange = false;
            for (var index = 0; index < statDataBuffer.Length; index++)
            {
                var statDataElement = statDataBuffer[index];
                if (!statDataElement.IsDirty) continue;
                hasStatChange = true;
                onStatChangeBuffer.Add(new StatChangeElement() { Value = statDataElement });
                statDataElement.IsDirty = false;
                statDataBuffer[index]   = statDataElement;
            }
    
            if (!hasStatChange) return;
            statChangeEnableState.ValueRW = true;
        }
    }
}