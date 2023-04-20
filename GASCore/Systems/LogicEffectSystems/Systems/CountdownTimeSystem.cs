namespace GASCore.Systems.LogicEffectSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using Unity.Burst;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityLogicEffectGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct CountdownTimeSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Duration>();
        }
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
    }

    [BurstCompile]
    public partial struct CountdownTimeJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;
        public float                              DeltaTime;

        void Execute(Entity abilityEntity, [EntityIndexInQuery] int entityInQueryIndex, ref Duration duration)
        {
            duration.Value -= this.DeltaTime;
            if (duration.Value <= 0) this.Ecb.SetComponentEnabled<Duration>(entityInQueryIndex, abilityEntity, false);
        }
    }
}