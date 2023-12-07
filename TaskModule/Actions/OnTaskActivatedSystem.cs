namespace TaskModule.Actions
{
    using TaskModule.TaskBase;
    using Unity.Burst;
    using Unity.Entities;

    [UpdateInGroup(typeof(TaskPresentationSystemGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct OnTaskActivatedSystem : ISystem
    {
        private EntityQuery activatedTaskQuery;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            this.activatedTaskQuery = SystemAPI.QueryBuilder().WithAll<ActivatedTag, TaskIndex, OnTaskActivatedAction>().Build();
            this.activatedTaskQuery.SetChangedVersionFilter(ComponentType.ReadOnly<ActivatedTag>());
            state.RequireForUpdate(this.activatedTaskQuery);
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            new OnTaskActivatedJob() { Ecb = ecb }.ScheduleParallel(this.activatedTaskQuery);
        }
    }

    [WithAll(typeof(ActivatedTag), typeof(TaskIndex))]
    [WithChangeFilter(typeof(ActivatedTag))]
    [BurstCompile]
    public partial struct OnTaskActivatedJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;
        void Execute([EntityIndexInQuery] int index, in DynamicBuffer<OnTaskActivatedAction> actionBuffer)
        {
            foreach (var action in actionBuffer)
            {
                this.Ecb.Instantiate(index, action.ActionEntityPrefab);
            }
        }
    }
}