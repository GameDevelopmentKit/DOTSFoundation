namespace QuestSystem.QuestActivateRequirement
{
    using QuestSystem.QuestBase;
    using TaskModule;
    using TaskModule.TaskBase;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;

    public struct ActiveTaskByTaskQuestActivated : IComponentData
    {
        public FixedString64Bytes QuestSource;
        public int                QuestId;
        public int                TaskOrder;
    }

    
    [UpdateInGroup(typeof(TaskInitializeSystemGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct ActiveTaskByTaskQuestActivatedSystem : ISystem
    {
        private EntityQuery triggerOnTaskActivated;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            this.triggerOnTaskActivated = SystemAPI.QueryBuilder().WithAll<ActiveTaskByTaskQuestActivated>().WithDisabled<ActivatedTag>().WithOptions(EntityQueryOptions.IncludeDisabledEntities)
                .Build();
            state.RequireForUpdate(this.triggerOnTaskActivated);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var triggerEntityArray               = this.triggerOnTaskActivated.ToEntityListAsync(state.WorldUpdateAllocator, out var getTriggerEntityHandle);
            var triggerOnTaskQuestActivatedArray = this.triggerOnTaskActivated.ToComponentDataListAsync<ActiveTaskByTaskQuestActivated>(state.WorldUpdateAllocator, out var getTriggerComponentHandle);
            var ecb                              = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            var listenOnHitEventJob = new ListenOnTaskActivatedJob()
            {
                TriggerEntityArray               = triggerEntityArray,
                TriggerOnTaskQuestActivatedArray = triggerOnTaskQuestActivatedArray,
                QuestInfoLookup                  = SystemAPI.GetComponentLookup<QuestInfo>(true),
                Ecb                              = ecb
            };

            state.Dependency = listenOnHitEventJob.ScheduleParallel(JobHandle.CombineDependencies(getTriggerComponentHandle, getTriggerEntityHandle));
        }

        [BurstCompile]
        [WithAll(typeof(ActivatedTag), typeof(TaskIndex))]
        [WithNone(typeof(QuestInfo))]
        [WithChangeFilter(typeof(ActivatedTag))]
        public partial struct ListenOnTaskActivatedJob : IJobEntity
        {
            [ReadOnly] public NativeList<Entity>                         TriggerEntityArray;
            [ReadOnly] public NativeList<ActiveTaskByTaskQuestActivated> TriggerOnTaskQuestActivatedArray;
            [ReadOnly] public ComponentLookup<QuestInfo>                 QuestInfoLookup;

            public EntityCommandBuffer.ParallelWriter Ecb;

            void Execute([EntityIndexInQuery] int index, in TaskIndex taskIndex, in ContainerOwner questEntityOwner)
            {
                for (var i = 0; i < this.TriggerOnTaskQuestActivatedArray.Length; i++)
                {
                    var triggerData = this.TriggerOnTaskQuestActivatedArray[i];
                    var questInfo   = this.QuestInfoLookup[questEntityOwner.Value];
                    if (triggerData.QuestSource == questInfo.QuestSource && triggerData.QuestId == questInfo.Id && triggerData.TaskOrder == taskIndex.Value)
                    {
                        this.Ecb.SetEnabled(index, this.TriggerEntityArray[i], true);
                        this.Ecb.SetComponentEnabled<ActivatedTag>(index, this.TriggerEntityArray[i], true);
                    }
                }
            }
        }
    }
}