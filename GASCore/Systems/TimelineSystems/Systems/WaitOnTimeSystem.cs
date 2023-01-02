namespace GASCore.Systems.TimelineSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.CommonSystems.Components;
    using GASCore.Systems.TimelineSystems.Components;
    using Unity.Burst;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityTimelineGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct WaitOnTimeSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton    = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb             = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            var timeElapsedTime = SystemAPI.Time.ElapsedTime;

            var setEndTimeTriggerAfterSecondJob = new SetEndTimeTriggerAfterSecondJob()
            {
                Ecb                = ecb,
                CurrentElapsedTime = timeElapsedTime
            };
           setEndTimeTriggerAfterSecondJob.ScheduleParallel();
        }
    }

    [BurstCompile]
    [WithNone(typeof(EndTimeComponent))]
    [WithChangeFilter(typeof(TriggerAfterSecond))]
    public partial struct SetEndTimeTriggerAfterSecondJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;
        public double                             CurrentElapsedTime;
        void Execute(Entity entity, [EntityInQueryIndex] int entityInQueryIndex, in TriggerAfterSecond triggerAfterSecond)
        {
            if (triggerAfterSecond.Second > 0)
            {
                this.Ecb.AddComponent(entityInQueryIndex, entity, new EndTimeComponent() { Value = this.CurrentElapsedTime + triggerAfterSecond.Second });
            }
        }
    }
}