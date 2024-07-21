namespace TaskModule.ActiveRequirement
{
    using TaskModule.TaskBase;
    using Unity.Burst;
    using Unity.Entities;

    [UpdateInGroup(typeof(TaskSimulationSystemGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct AutoActivateTaskOnStartSystem : ISystem
    {
        private EntityQuery taskQuery;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            this.taskQuery = SystemAPI.QueryBuilder().WithAll<AutoActiveOnStartTag>().WithNone<ContainerOwner>().WithDisabled<ActivatedTag>().WithOptions(EntityQueryOptions.IncludeDisabledEntities)
                .Build();
            state.RequireForUpdate(this.taskQuery);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if(this.taskQuery.IsEmpty) return;
            new ActivateTaskJob()
            {
                Ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
            }.ScheduleParallel(this.taskQuery);
        }


        public partial struct ActivateTaskJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter Ecb;

            void Execute(Entity entity, [EntityIndexInQuery] int index)
            {
                this.Ecb.SetEnabled(index, entity, true);
                this.Ecb.SetComponentEnabled<ActivatedTag>(index, entity, true);
            }
        }
    }
}