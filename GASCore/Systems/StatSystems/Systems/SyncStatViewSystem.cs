namespace GASCore.Systems.StatSystems.Systems
{
    using DOTSCore.CommonSystems.Components;
    using GASCore.Groups;
    using GASCore.Systems.StatSystems.Components;
    using GASCore.Systems.TimelineSystems.Components;
    using Unity.Burst;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityCleanupSystemGroup))]
    [UpdateBefore(typeof(CleanupOnStatChangeEventSystem))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial class SyncStatViewSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithoutBurst().WithAll<OnStatChangeTag, ListenerCollector>().ForEach((in EventQueue eventQueue, in DynamicBuffer<StatChangeElement> onStatChangeBuffer) =>
            {
                foreach (var onStatChange in onStatChangeBuffer)
                {
                    eventQueue.Value.Enqueue(new ChangeStatEvent() { ChangedStat = onStatChange.Value });
                }
            }).Run();
        }
    }

    public struct ChangeStatEvent
    {
        public StatDataElement ChangedStat;
    }
}