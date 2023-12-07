namespace TaskModule.Actions
{
    using TaskModule.TaskBase;
    using Unity.Burst;
    using Unity.Entities;

    public struct SetActiveOnTaskCompleted : IComponentData
    {
        public Entity TargetEntity;
        public bool Value;
    }

    [UpdateInGroup(typeof(TaskPresentationSystemGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct SetActiveEntitySystem : ISystem
    {
        private EntityQuery completedTaskQuery;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            this.completedTaskQuery = SystemAPI.QueryBuilder().WithAll<CompletedTag, SetActiveOnTaskCompleted>().WithOptions(EntityQueryOptions.IncludeDisabledEntities).Build();
            this.completedTaskQuery.SetChangedVersionFilter(ComponentType.ReadOnly<CompletedTag>());
            state.RequireForUpdate(this.completedTaskQuery);
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            
            new SetActiveEntityJob()
            {
                Ecb = ecb
            }.ScheduleParallel(this.completedTaskQuery);
        }
    }

    [WithAll(typeof(CompletedTag))]
    [WithChangeFilter(typeof(CompletedTag))]
    [WithOptions(EntityQueryOptions.IncludeDisabledEntities)]
    [BurstCompile]
    public partial struct SetActiveEntityJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;

        public void Execute([EntityIndexInQuery] int index, in SetActiveOnTaskCompleted data) { this.Ecb.SetEnabled(index, data.TargetEntity, data.Value); }
    }
}