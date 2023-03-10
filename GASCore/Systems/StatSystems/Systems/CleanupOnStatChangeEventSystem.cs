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
        public void OnCreate(ref SystemState state) { }
        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            new CleanupOnStatChangeEventJob() { Ecb    = ecb }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct CleanupOnStatChangeEventJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;

        void Execute(Entity entity, [EntityIndexInQuery] int entityInQueryIndex, ref DynamicBuffer<OnStatChange> statChangeEventBuffer)
        {
            statChangeEventBuffer.Clear();
            this.Ecb.SetComponentEnabled<OnStatChange>(entityInQueryIndex, entity, false);
        }
    }
}