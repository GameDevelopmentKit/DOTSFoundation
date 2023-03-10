namespace DOTSCore.CommonSystems.Systems
{
    using DOTSCore.CommonSystems.Components;
    using Unity.Burst;
    using Unity.Entities;

    /// <summary>
    /// Destroy the notify entity at the end of loop, to let the other system can listen this notify
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup), OrderLast = true)]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct CleanupAllNotifyEntitySystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { state.RequireForUpdate<NotifyComponentTag>(); }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<BeginPresentationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            new CleanupAllNotifyEntityJob() { Ecb = ecb }.ScheduleParallel();
        }
    }

    [BurstCompile]
    [WithAll(typeof(NotifyComponentTag))]
    public partial struct CleanupAllNotifyEntityJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;

        void Execute(Entity abilityActionEntity, [EntityIndexInQuery] int entityInQueryIndex) { this.Ecb.DestroyEntity(entityInQueryIndex, abilityActionEntity); }
    }
}