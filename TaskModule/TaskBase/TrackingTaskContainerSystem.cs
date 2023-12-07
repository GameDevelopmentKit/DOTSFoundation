namespace TaskModule.TaskBase
{
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using ISystem = Unity.Entities.ISystem;

    [UpdateInGroup(typeof(TaskInitializeSystemGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct TrackingTaskContainerSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new NotifyTaskContainerJob()
            {
                OnRequirementMetLookup = SystemAPI.GetComponentLookup<OnSubTaskElementCompleted>()
            }.ScheduleParallel();

            new TrackingTaskContainerProgressJob()
            {
                OptionalTagLookup = SystemAPI.GetComponentLookup<OptionalTag>(true),
                CompetedTagLookup = SystemAPI.GetComponentLookup<CompletedTag>()
            }.ScheduleParallel();
        }
    }

    [WithAll(typeof(CompletedTag))]
    [WithChangeFilter(typeof(CompletedTag))]
    [BurstCompile]
    public partial struct NotifyTaskContainerJob : IJobEntity
    {
        [NativeDisableParallelForRestriction] public ComponentLookup<OnSubTaskElementCompleted> OnRequirementMetLookup;
        void Execute(in ContainerOwner owner)
        {
            if (!this.OnRequirementMetLookup.IsComponentEnabled(owner.Value))
            {
                this.OnRequirementMetLookup.SetComponentEnabled(owner.Value, true);
            }
        }
    }

    [WithAll(typeof(OnSubTaskElementCompleted))]
    [WithNone(typeof(CompletedTag))]
    [BurstCompile]
    public partial struct TrackingTaskContainerProgressJob : IJobEntity
    {
        [NativeDisableParallelForRestriction] public ComponentLookup<CompletedTag> CompetedTagLookup;
        [ReadOnly]                            public ComponentLookup<OptionalTag>  OptionalTagLookup;
        void Execute(Entity entity, EnabledRefRW<OnSubTaskElementCompleted> onEventState, in TaskContainerSetting setting, in DynamicBuffer<SubTaskEntity> subTaskEntities,
            ref TaskProgress taskProgress)
        {
            onEventState.ValueRW = false;
            var numOptionalMet = 0;
            taskProgress.Value = 0;
            foreach (var subTask in subTaskEntities)
            {
                var isOptional  = this.OptionalTagLookup.HasComponent(subTask.Value);
                var isCompleted = this.CompetedTagLookup.IsComponentEnabled(subTask.Value);
                if (isCompleted)
                {
                    taskProgress.Value += 1f / subTaskEntities.Length;
                    if (isOptional) numOptionalMet++;
                }
                else
                {
                    if (!isOptional) return;
                }
            }

            if (numOptionalMet >= setting.RequireOptionalAmount)
            {
                this.CompetedTagLookup.SetComponentEnabled(entity, true);
            }
        }
    }
}