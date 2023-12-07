namespace DOTSCore.CommonSystems.Systems
{
    using DOTSCore.CommonSystems.Components;
    using Unity.Entities;

    [UpdateInGroup(typeof(PresentationSystemGroup), OrderLast = true)]
    [RequireMatchingQueriesForUpdate]
    public partial class DispatchEventToListenerSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            //Sync view event
            this.Entities.WithoutBurst().ForEach((in EventQueue eventQueue, in ListenerCollector listenerCollector) =>
            {
                if (eventQueue.Value.Count <= 0) return;
                var valueCount = eventQueue.Value.Count;
                for (var i = 0; i < valueCount; i++)
                {
                    listenerCollector.Dispatch(eventQueue.Value.Dequeue());
                }
            }).Run();
        }
    }
}