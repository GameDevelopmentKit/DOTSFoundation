namespace GASCore.Systems.LogicEffectSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.LogicEffectSystems.Components;
    using Unity.Burst;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityCommonSystemGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct WaitEndTimeSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)  {  }
        
        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton       = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb                = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            var currentElapsedTime = SystemAPI.Time.ElapsedTime;
            
            new WaitEndTimeJob()
            {
                Ecb = ecb ,
                CurrentElapsedTime = currentElapsedTime
            }.ScheduleParallel();
        }
    }
    
    [BurstCompile]
    public partial struct WaitEndTimeJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;
        public double                             CurrentElapsedTime;
        
        void Execute(Entity entity, [EntityInQueryIndex] int entityInQueryIndex,in EndTimeComponent endTime)
        {
            if (this.CurrentElapsedTime >= endTime.Value)
            {
                this.Ecb.SetComponentEnabled<EndTimeComponent>(entityInQueryIndex, entity, false);
            }
        }
    }
}