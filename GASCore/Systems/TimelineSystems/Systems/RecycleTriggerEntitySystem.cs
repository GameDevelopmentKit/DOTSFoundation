namespace GASCore.Systems.TimelineSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
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

            new RecycleTriggerEntityJob1(){ Ecb = ecb }.ScheduleParallel();
            new RecycleTriggerEntityJob2(){ Ecb = ecb }.ScheduleParallel();
        }
    }

    [BurstCompile]
    [WithAll(typeof(RecycleTriggerEntityTag), typeof(CreateAbilityEffectElement))]
    [WithNone(typeof(TriggerConditionCount))]
    public partial struct RecycleTriggerEntityJob1 : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;
        void Execute(Entity entity, [EntityInQueryIndex] int entityInQueryIndex, in TriggerConditionAmount triggerConditionAmount)
        {
            this.Ecb.SetComponentEnabled<TriggerConditionCount>(entityInQueryIndex, entity, true);
            this.Ecb.SetComponent(entityInQueryIndex, entity, new TriggerConditionCount() { Value = triggerConditionAmount.Value });
            this.Ecb.SetComponentEnabled<WaitingCreateEffect>(entityInQueryIndex, entity, true);
        }
    }

    [BurstCompile]
    [WithAll(typeof(RecycleTriggerEntityTag))]
    [WithNone(typeof(TriggerConditionCount), typeof(CreateAbilityEffectElement))]
    public partial struct RecycleTriggerEntityJob2 : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;
        void Execute(Entity entity, [EntityInQueryIndex] int entityInQueryIndex, in TriggerConditionAmount triggerConditionAmount)
        {
            this.Ecb.SetComponentEnabled<TriggerConditionCount>(entityInQueryIndex, entity, true);
            this.Ecb.SetComponent(entityInQueryIndex, entity, new TriggerConditionCount() { Value = triggerConditionAmount.Value });
        }
    }
}