namespace GASCore.Systems.TimelineSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.TimelineSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;

    [UpdateInGroup(typeof(GameAbilityInitializeSystemGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct RecycleTriggerEntitySystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            using var entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<RecycleTriggerEntityTag,CompletedAllTriggerConditionTag>();
            state.RequireForUpdate(state.GetEntityQuery(entityQuery));
        }

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
        void Execute(Entity entity, [EntityIndexInQuery] int entityInQueryIndex, ref DynamicBuffer<CompletedTriggerElement> completedTriggerBuffer)
        {
            this.Ecb.SetComponentEnabled<CompletedAllTriggerConditionTag>(entityInQueryIndex, entity, false);
            this.Ecb.SetComponentEnabled<InTriggerConditionResolveProcessTag>(entityInQueryIndex, entity, true);
            completedTriggerBuffer.Clear();
        }
    }
}