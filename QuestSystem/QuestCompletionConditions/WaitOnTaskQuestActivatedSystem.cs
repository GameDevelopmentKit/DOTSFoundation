namespace QuestSystem.QuestCompletionConditions
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
    public partial struct WaitOnTaskQuestActivatedSystem : ISystem
    {
        private EntityQuery triggerOnTaskActivated;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            this.triggerOnTaskActivated = SystemAPI.QueryBuilder().WithAll<TriggerOnTaskQuestActivated>().WithNone<CompletedTag>().WithOptions(EntityQueryOptions.IncludeDisabledEntities).Build();
            state.RequireForUpdate(this.triggerOnTaskActivated);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var triggerEntityArray               = this.triggerOnTaskActivated.ToEntityListAsync(state.WorldUpdateAllocator, out var getTriggerEntityHandle);
            var triggerOnTaskQuestActivatedArray = this.triggerOnTaskActivated.ToComponentDataListAsync<TriggerOnTaskQuestActivated>(state.WorldUpdateAllocator, out var getTriggerComponentHandle);

            var listenOnHitEventJob = new ListenOnTaskActivatedJob()
            {
                TriggerEntityArray               = triggerEntityArray,
                TriggerOnTaskQuestActivatedArray = triggerOnTaskQuestActivatedArray,
                QuestInfoLookup                  = SystemAPI.GetComponentLookup<QuestInfo>(true),
                CompletedLookup                  = SystemAPI.GetComponentLookup<CompletedTag>()
            };

            state.Dependency = listenOnHitEventJob.ScheduleParallel(JobHandle.CombineDependencies(getTriggerComponentHandle, getTriggerEntityHandle));
        }


        [BurstCompile]
        [WithAll(typeof(ActivatedTag), typeof(TaskIndex))]
        [WithNone(typeof(QuestInfo))]
        [WithChangeFilter(typeof(ActivatedTag))]
        public partial struct ListenOnTaskActivatedJob : IJobEntity
        {
            [ReadOnly] public NativeList<Entity>                      TriggerEntityArray;
            [ReadOnly] public NativeList<TriggerOnTaskQuestActivated> TriggerOnTaskQuestActivatedArray;
            [ReadOnly] public ComponentLookup<QuestInfo>              QuestInfoLookup;

            [NativeDisableParallelForRestriction] public ComponentLookup<CompletedTag> CompletedLookup;

            void Execute(in TaskIndex taskIndex, in ContainerOwner questEntityOwner)
            {
                for (var i = 0; i < this.TriggerOnTaskQuestActivatedArray.Length; i++)
                {
                    var triggerData = this.TriggerOnTaskQuestActivatedArray[i];
                    var questInfo   = this.QuestInfoLookup[questEntityOwner.Value];
                    if (triggerData.QuestSource == questInfo.QuestSource && triggerData.QuestId == questInfo.Id && triggerData.TaskOrder == taskIndex.Value)
                        this.CompletedLookup.SetComponentEnabled(this.TriggerEntityArray[i], true);
                }
            }
        }
    }
}