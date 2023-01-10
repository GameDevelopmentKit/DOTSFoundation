namespace GASCore.Systems.LogicEffectSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.CommonSystems.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using GASCore.Systems.TimelineSystems.Components;
    using Unity.Burst;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityTimelineGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct FindNearestTargetAtDestinationSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new ManageTriggerConditionJob().ScheduleParallel(state.Dependency);
        }
    }

    [WithAll(typeof(ArrivedAtDestinationTag))]
    [WithAll(typeof(FindNearestTargetAtDestination))]
    [BurstCompile]
    internal partial struct ManageTriggerConditionJob : IJobEntity
    {
        private void Execute(Entity entity, ref TriggerConditionCount triggerConditionCount, ref DynamicBuffer<TargetableElement> targets)
        {
            targets.Add(new TargetableElement() { Value = entity });
            --triggerConditionCount.Value;
        }
    }
}