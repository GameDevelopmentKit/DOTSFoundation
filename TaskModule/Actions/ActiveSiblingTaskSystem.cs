namespace TaskModule.Actions
{
    using TaskModule.TaskBase;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;

    [UpdateInGroup(typeof(TaskPresentationSystemGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct ActiveSiblingTaskSystem : ISystem
    {
        private EntityQuery completedTaskQuery;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            this.completedTaskQuery = SystemAPI.QueryBuilder().WithAll<CompletedTag, ActiveSiblingTaskOnComplete, ContainerOwner>().Build();
            this.completedTaskQuery.SetChangedVersionFilter(ComponentType.ReadOnly<CompletedTag>());
            state.RequireForUpdate(this.completedTaskQuery);
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if(this.completedTaskQuery.IsEmpty) return;
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            new ActiveSiblingTaskJob()
            {
                Ecb           = ecb,
                SubTaskLookup = SystemAPI.GetBufferLookup<SubTaskEntity>(true)
            }.ScheduleParallel(this.completedTaskQuery);
        }
    }

    [WithAll(typeof(CompletedTag))]
    [WithChangeFilter(typeof(CompletedTag))]
    [BurstCompile]
    public partial struct ActiveSiblingTaskJob : IJobEntity
    {
        public            EntityCommandBuffer.ParallelWriter Ecb;
        [ReadOnly] public BufferLookup<SubTaskEntity>        SubTaskLookup;

        void Execute([EntityIndexInQuery] int index, in ActiveSiblingTaskOnComplete activeSiblingTask, in ContainerOwner containerOwner)
        {
            var subTasks = this.SubTaskLookup[containerOwner.Value];
            if (activeSiblingTask.TaskOrder >= 0 && activeSiblingTask.TaskOrder < subTasks.Length)
            {
                var nextTaskEntity = subTasks[activeSiblingTask.TaskOrder].Value;
                this.Ecb.SetEnabled(index, nextTaskEntity, true);
                this.Ecb.SetComponentEnabled<ActivatedTag>(index, nextTaskEntity, true);
            }
        }
    }
}