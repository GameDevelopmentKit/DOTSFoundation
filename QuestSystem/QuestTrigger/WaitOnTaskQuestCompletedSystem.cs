namespace QuestSystem.QuestTrigger
{
    using QuestSystem.QuestBase;
    using TaskModule;
    using TaskModule.TaskBase;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;

    [UpdateInGroup(typeof(TaskInitializeSystemGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct WaitOnTaskQuestCompletedSystem : ISystem
    {
        private EntityQuery triggerOnTaskActivated;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            this.triggerOnTaskActivated   = SystemAPI.QueryBuilder().WithAll<TriggerOnTaskQuestCompleted>().WithNone<CompletedTag>().Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var triggerEntityArray               = this.triggerOnTaskActivated.ToEntityListAsync(state.WorldUpdateAllocator, out var getTriggerEntityHandle);
            var triggerOnTaskQuestActivatedArray = this.triggerOnTaskActivated.ToComponentDataListAsync<TriggerOnTaskQuestCompleted>(state.WorldUpdateAllocator, out var getTriggerComponentHandle);

            var listenOnHitEventJob = new ListenOnTaskCompletedJob()
            {
                TriggerEntityArray               = triggerEntityArray,
                TriggerOnTaskQuestCompletedArray = triggerOnTaskQuestActivatedArray,
                QuestInfoLookup                  = SystemAPI.GetComponentLookup<QuestInfo>(true),
                CompletedLookup                  = SystemAPI.GetComponentLookup<CompletedTag>()
            };

            state.Dependency = listenOnHitEventJob.ScheduleParallel(JobHandle.CombineDependencies(getTriggerComponentHandle, getTriggerEntityHandle));
        }
    }


    //todo research generic job
    [BurstCompile]
    [WithAll(typeof(CompletedTag), typeof(TaskIndex))]
    [WithNone(typeof(QuestInfo))]
    [WithChangeFilter(typeof(CompletedTag))]
    public partial struct ListenOnTaskCompletedJob : IJobEntity
    {
        [ReadOnly] public NativeList<Entity>                      TriggerEntityArray;
        [ReadOnly] public NativeList<TriggerOnTaskQuestCompleted> TriggerOnTaskQuestCompletedArray;
        [ReadOnly] public ComponentLookup<QuestInfo>              QuestInfoLookup;

        [NativeDisableParallelForRestriction] public ComponentLookup<CompletedTag> CompletedLookup;

        void Execute(in TaskIndex taskIndex, in ContainerOwner questEntityOwner)
        {
            for (var i = 0; i < this.TriggerOnTaskQuestCompletedArray.Length; i++)
            {
                var triggerData = this.TriggerOnTaskQuestCompletedArray[i];
                var questInfo   = this.QuestInfoLookup[questEntityOwner.Value];
                if (triggerData.QuestSource == questInfo.QuestSource && triggerData.QuestId == questInfo.Id && triggerData.TaskOrder == taskIndex.Value)
                    this.CompletedLookup.SetComponentEnabled(this.TriggerEntityArray[i], true);
            }
        }
    }
}