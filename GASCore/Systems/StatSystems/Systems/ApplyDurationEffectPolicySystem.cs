namespace GASCore.Systems.StatSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.StatSystems.Components;
    using Unity.Burst;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityLogicEffectGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct ApplyDurationEffectPolicySystem : ISystem
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
            new ApplyDurationEffectPolicyJob() { Ecb = ecb }.ScheduleParallel();
        }
    }

    [BurstCompile]
    [WithNone(typeof(Duration))]
    public partial struct ApplyDurationEffectPolicyJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;

        void Execute(Entity statModifierEntity, [EntityInQueryIndex] int entityInQueryIndex, in DurationEffect durationEffect)
        {
            this.Ecb.AddComponent(entityInQueryIndex, statModifierEntity, new Duration() { Value = durationEffect.Value });
        }
    }
}