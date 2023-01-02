namespace GASCore.Systems.TimelineSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.TimelineSystems.Components;
    using Unity.Burst;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityTimelineGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct DisableTriggerConditionCountSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            state.Dependency = new RemoveTriggerConditionJob()
            {
                Ecb = ecb,
            }.ScheduleParallel(state.Dependency);
        }
    }

    [WithChangeFilter(typeof(TriggerConditionCount))]
    public partial struct RemoveTriggerConditionJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;
        void Execute(Entity entity, [EntityInQueryIndex] int entityInQueryIndex, in TriggerConditionCount count)
        {
            if (count.Value <= 0)
            {
                this.Ecb.SetComponentEnabled<TriggerConditionCount>(entityInQueryIndex, entity, false);
            }
        }
    }
}