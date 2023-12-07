namespace TaskModule.Actions
{
    using TaskModule.TaskBase;
    using Unity.Burst;
    using Unity.Entities;

    [UpdateInGroup(typeof(TaskPresentationSystemGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct OnTaskCompletedSystem : ISystem
    {
        private EntityQuery completedTaskQuery;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            this.completedTaskQuery = SystemAPI.QueryBuilder().WithAll<CompletedTag, TaskIndex, OnTaskCompletedAction>().Build();
            this.completedTaskQuery.SetChangedVersionFilter(ComponentType.ReadOnly<CompletedTag>());
            state.RequireForUpdate(this.completedTaskQuery);
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            new OnTaskCompletedJob() { Ecb = ecb }.ScheduleParallel(this.completedTaskQuery);
        }
    }
    
    [WithAll(typeof(CompletedTag), typeof(TaskIndex))]
    [WithChangeFilter(typeof(CompletedTag))]
    [BurstCompile]
    public partial struct OnTaskCompletedJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;
        void Execute([EntityIndexInQuery] int index, in DynamicBuffer<OnTaskCompletedAction> actionBuffer)
        {
            foreach (var action in actionBuffer)
            {
                this.Ecb.Instantiate(index, action.ActionEntityPrefab);
            }
        }
    }
}