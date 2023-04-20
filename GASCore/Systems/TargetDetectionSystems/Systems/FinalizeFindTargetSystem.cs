namespace GASCore.Systems.TargetDetectionSystems.Systems
{
    using GASCore.Systems.TargetDetectionSystems.Components;
    using GASCore.Systems.TimelineSystems.Components;
    using Unity.Burst;
    using Unity.Entities;

    [UpdateInGroup(typeof(FinalizeFindTargetGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct FinalizeFindTargetSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<FindTargetComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            state.Dependency = new FinalizeFindTargetJob
            {
                Ecb = ecb,
            }.ScheduleParallel(state.Dependency);
        }
    }

    [WithAll(typeof(FindTargetComponent))]
    [BurstCompile]
    public partial struct FinalizeFindTargetJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;

        private void Execute(
            Entity entity,
            [EntityIndexInQuery] int index,
            in DynamicBuffer<TargetableElement> targetables,
            ref DynamicBuffer<CompletedTriggerElement> completedTriggers
        )
        {
            if (targetables.IsEmpty) return;
            this.Ecb.SetComponentEnabled<FindTargetComponent>(index, entity, false);
            completedTriggers.Add(new CompletedTriggerElement { Index = TypeManager.GetTypeIndex<FindTargetComponent>() });
        }
    }
}