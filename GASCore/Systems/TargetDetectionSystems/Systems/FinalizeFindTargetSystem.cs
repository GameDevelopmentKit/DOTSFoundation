namespace GASCore.Systems.TargetDetectionSystems.Systems
{
    using GASCore.Systems.TargetDetectionSystems.Components;
    using GASCore.Systems.TimelineSystems.Components;
    using Unity.Burst;
    using Unity.Entities;

    [UpdateInGroup(typeof(FinalizeFindTargetGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct FinalizeFindTargetSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state) { state.Dependency = new FinalizeFindTargetJob().ScheduleParallel(state.Dependency); }
    }

    [WithAll(typeof(FindTargetComponent))]
    [BurstCompile]
    public partial struct FinalizeFindTargetJob : IJobEntity
    {
        private void Execute(
            in DynamicBuffer<TargetableElement> targetables,
            ref DynamicBuffer<CompletedTriggerElement> completedTriggers,
            EnabledRefRW<FindTargetComponent> findTargetEnableState
        )
        {
            if (targetables.IsEmpty) return;
            findTargetEnableState.ValueRW = false;
            completedTriggers.Add(new CompletedTriggerElement { Index = TypeManager.GetTypeIndex<FindTargetComponent>() });
        }
    }
}