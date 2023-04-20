namespace GASCore.Systems.TimelineSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.TimelineSystems.Components;
    using Unity.Burst;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityTimelineGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct WaitOnTimeSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timeElapsedTime = SystemAPI.Time.ElapsedTime;

            var setEndTimeTriggerAfterSecondJob = new SetEndTimeTriggerAfterSecondJob()
            {
                CurrentElapsedTime = timeElapsedTime
            };
           setEndTimeTriggerAfterSecondJob.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct SetEndTimeTriggerAfterSecondJob : IJobEntity
    {
        public double                             CurrentElapsedTime;
        void Execute(ref TriggerAfterSecond triggerAfterSecond, ref DynamicBuffer<CompletedTriggerElement> completedTriggerBuffer)
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
                completedTriggerBuffer.Add(new CompletedTriggerElement() { Index = TypeManager.GetTypeIndex<TriggerAfterSecond>() });
            }
        }
    }
}