namespace TutorialSystem.Systems
{
    using TaskModule;
    using TaskModule.TaskBase;
    using TutorialSystem.Components;
    using Unity.Burst;
    using Unity.Entities;

    [UpdateInGroup(typeof(TaskPresentationSystemGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct WaitTimeSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new WaitTimeJob()
            {
                CurrentElapsedTime = SystemAPI.Time.ElapsedTime
            }.Schedule();
        }
    }

    [BurstCompile]
    [WithAll(typeof(TaskIndex), typeof(ActivatedTag))]
    [WithDisabled(typeof(CompletedTag))]
    public partial struct WaitTimeJob : IJobEntity
    {
        public double CurrentElapsedTime;

        void Execute(ref WaitTime waitTime, EnabledRefRW<CompletedTag> enabledCompletedTag)
        {
            if (waitTime.NextEndTimeValue <= 0)
            {
                waitTime.NextEndTimeValue = this.CurrentElapsedTime + waitTime.Seconds;
            }

            if (this.CurrentElapsedTime >= waitTime.NextEndTimeValue)
            {
                waitTime.NextEndTimeValue   = -1;
                enabledCompletedTag.ValueRW = true;
            }
        }
    }
}