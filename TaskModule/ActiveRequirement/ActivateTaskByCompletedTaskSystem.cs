namespace TaskModule.ActiveRequirement
{
    using TaskModule.TaskBase;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;

    [UpdateInGroup(typeof(TaskSimulationSystemGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct ActivateTaskByCompletedTaskSystem : ISystem
    {
        private EntityQuery completedTaskQuery;
        private EntityQuery taskQuery;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            this.taskQuery = SystemAPI.QueryBuilder().WithAll<ActivateByCompletedTask, TaskIndex, ContainerOwner>().WithDisabled<ActivatedTag>().WithOptions(EntityQueryOptions.IncludeDisabledEntities)
                .Build();
            state.RequireForUpdate(this.taskQuery);

            this.completedTaskQuery = SystemAPI.QueryBuilder().WithAll<CompletedTag, TaskIndex>().Build();
            this.completedTaskQuery.SetChangedVersionFilter(ComponentType.ReadOnly<CompletedTag>());
            state.RequireForUpdate(this.completedTaskQuery);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (this.completedTaskQuery.IsEmpty) return;

            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            new ActivateTaskJob()
            {
                Ecb             = ecb,
                SubTaskLookup   = SystemAPI.GetBufferLookup<SubTaskEntity>(true),
                CompletedLookup = SystemAPI.GetComponentLookup<CompletedTag>(true)
            }.ScheduleParallel(this.taskQuery);
        }


        public partial struct ActivateTaskJob : IJobEntity
        {
            [ReadOnly] public BufferLookup<SubTaskEntity>   SubTaskLookup;
            [ReadOnly] public ComponentLookup<CompletedTag> CompletedLookup;

            public EntityCommandBuffer.ParallelWriter Ecb;

            void Execute(Entity entity, [EntityIndexInQuery] int index, ref DynamicBuffer<ActivateByCompletedTask> activateByCompletedTasks, in ContainerOwner containerOwner)
            {
                var subTasks = this.SubTaskLookup[containerOwner.Value];
                for (var i = 0; i < activateByCompletedTasks.Length;)
                {
                    var activateByCompletedTask = activateByCompletedTasks[i];
                    if (activateByCompletedTask.TaskIndex >= 0 && activateByCompletedTask.TaskIndex < subTasks.Length)
                    {
                        var taskEntity = subTasks[activateByCompletedTask.TaskIndex].Value;
                        if (this.CompletedLookup.IsComponentEnabled(taskEntity))
                        {
                            activateByCompletedTasks.RemoveAtSwapBack(i);
                            continue;
                        }
                    }

                    i++;
                }

                if (activateByCompletedTasks.Length != 0) return;
                this.Ecb.SetEnabled(index, entity, true);
                this.Ecb.SetComponentEnabled<ActivatedTag>(index, entity, true);
            }
        }
    }
}