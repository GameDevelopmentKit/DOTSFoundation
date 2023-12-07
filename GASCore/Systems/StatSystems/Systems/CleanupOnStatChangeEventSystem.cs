namespace GASCore.Systems.StatSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Systems.AbilitySystem;
    using GASCore.Systems.TimelineSystems.Components;
    using Unity.Burst;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityCleanupSystemGroup))]
    [UpdateBefore(typeof(CleanupUnusedAbilityEntitiesSystem))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct CleanupOnStatChangeEventSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state) { new CleanupOnStatChangeEventJob().ScheduleParallel(); }
    }

    [BurstCompile]
    public partial struct CleanupOnStatChangeEventJob : IJobEntity
    {
        void Execute(ref DynamicBuffer<StatChangeElement> statChangeEventBuffer, EnabledRefRW<OnStatChangeTag> statChangeEnableState)
        {
            statChangeEventBuffer.Clear();
            statChangeEnableState.ValueRW = false;
        }
    }
}