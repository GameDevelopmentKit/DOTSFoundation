namespace GASCore.Systems.TimelineSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.TimelineSystems.Components;
    using Unity.Burst;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityTimelineGroup))]
    [UpdateAfter(typeof(InstantiateAbilityEffectFromPoolSystem))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct RecycleTriggerEntitySystem : ISystem
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

            new RecycleTriggerEntityJob(){ Ecb = ecb }.ScheduleParallel();
        }
    }

    [BurstCompile]
    [WithAll(typeof(RecycleTriggerEntityTag), typeof(CompletedAllTriggerConditionTag))]
    public partial struct RecycleTriggerEntityJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;
        void Execute(Entity entity, [EntityInQueryIndex] int entityInQueryIndex, ref DynamicBuffer<CompletedTriggerElement> completedTriggerBuffer)
        {
            this.Ecb.SetComponentEnabled<CompletedAllTriggerConditionTag>(entityInQueryIndex, entity, false);
            this.Ecb.SetComponentEnabled<InTriggerConditionResolveProcessTag>(entityInQueryIndex, entity, true);
            completedTriggerBuffer.Clear();
        }
    }
}