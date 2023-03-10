namespace GASCore.Systems.StatSystems.Systems
{
    using DOTSCore.CommonSystems.Components;
    using GASCore.Groups;
    using GASCore.Systems.StatSystems.Components;
    using GASCore.Systems.TimelineSystems.Components;
    using Unity.Burst;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityVisualEffectGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial class SyncStatViewSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithoutBurst().ForEach((Entity entity, in DynamicBuffer<OnStatChange> onStatChangeBuffer) =>
            {
                foreach (var onStatChange in onStatChangeBuffer)
                {
                    EntityManager.TryEnqueueViewEvent(entity, new ChangeStatEvent() { ChangedStat = onStatChange.ChangedStat });
                }
            }).Run();
        }
    }

    public struct ChangeStatEvent
    {
        public StatDataElement ChangedStat;
    }
}