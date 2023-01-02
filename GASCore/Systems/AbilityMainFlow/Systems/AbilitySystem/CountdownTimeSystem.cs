namespace GASCore.Systems.AbilityMainFlow.Systems.AbilitySystem
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using Unity.Burst;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityMainFlowGroup), OrderFirst = true)]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct CountdownTimeSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { }
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            var deltaTime    = SystemAPI.Time.DeltaTime;
            new CountdownTimeJob()
            {
                Ecb       = ecb,
                DeltaTime = deltaTime
            }.ScheduleParallel();
        }
        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }

    [BurstCompile]
    public partial struct CountdownTimeJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;
        public float                              DeltaTime;

        void Execute(Entity abilityEntity, [EntityInQueryIndex] int entityInQueryIndex, ref Duration duration)
        {
            duration.Value -= this.DeltaTime;
            if (duration.Value <= 0) this.Ecb.SetComponentEnabled<Duration>(entityInQueryIndex, abilityEntity, false);
        }
    }
}