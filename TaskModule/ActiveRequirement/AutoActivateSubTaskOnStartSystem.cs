namespace TaskModule.ActiveRequirement
{
    using TaskModule.TaskBase;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;

    [UpdateInGroup(typeof(TaskSimulationSystemGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct AutoActivateSubTaskOnStartSystem : ISystem
    {
        private EntityQuery taskQuery;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            this.taskQuery = SystemAPI.QueryBuilder().WithAll<ActivatedTag,TaskIndex, SubTaskEntity>().Build();
            this.taskQuery.SetChangedVersionFilter(ComponentType.ReadOnly<ActivatedTag>());
            state.RequireForUpdate(this.taskQuery);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if(this.taskQuery.IsEmpty) return;
            new AutoActivateSubTaskJob()
            {
                Ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
                AutoActiveOnStartTagLookup = SystemAPI.GetComponentLookup<AutoActiveOnStartTag>(true)
            }.ScheduleParallel(this.taskQuery);
        }

        [WithAll(typeof(ActivatedTag), typeof(TaskIndex))]
        [WithChangeFilter(typeof(ActivatedTag))]
        [BurstCompile]
        public partial struct AutoActivateSubTaskJob : IJobEntity
        {
            public            EntityCommandBuffer.ParallelWriter    Ecb;
            [ReadOnly] public ComponentLookup<AutoActiveOnStartTag> AutoActiveOnStartTagLookup;
            void Execute([EntityIndexInQuery] int index, in DynamicBuffer<SubTaskEntity> subTaskEntities)
            {
                foreach (var subTask in subTaskEntities)
                {
                    if (this.AutoActiveOnStartTagLookup.HasComponent(subTask.Value))
                    {
                        this.Ecb.SetEnabled(index, subTask.Value, true);
                        this.Ecb.SetComponentEnabled<ActivatedTag>(index, subTask.Value, true);
                    }
                }
            }
        }
    }
}