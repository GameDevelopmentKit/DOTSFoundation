namespace GASCore.Systems.TimelineSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Services;
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
    public partial struct SetEndTimeTriggerAfterSecondJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;
        public double                             CurrentElapsedTime;
        void Execute(Entity entity, [EntityInQueryIndex] int entityInQueryIndex, ref TriggerAfterSecond triggerAfterSecond, in DynamicBuffer<CompletedTriggerElement> completedTriggerBuffer)
        {
            if (completedTriggerBuffer.Length > 0)
            {
                foreach (var completedTrigger in completedTriggerBuffer)
                {
                    // return if TriggerAfterSecond already completed
                    if (completedTrigger.Index == TypeManager.GetTypeIndex<TriggerAfterSecond>()) return;
                }
            }

            if (triggerAfterSecond.EndTime == 0)
            {
                triggerAfterSecond.EndTime = this.CurrentElapsedTime + triggerAfterSecond.Second;
            }
            
            if (this.CurrentElapsedTime >= triggerAfterSecond.EndTime)
            {
                triggerAfterSecond.EndTime = 0;
                this.Ecb.MarkTriggerConditionComplete<TriggerAfterSecond>(entity, entityInQueryIndex);
            }
        }
    }
}