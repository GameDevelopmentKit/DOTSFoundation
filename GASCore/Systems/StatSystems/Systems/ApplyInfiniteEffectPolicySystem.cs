namespace GASCore.Systems.StatSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.StatSystems.Components;
    using Unity.Burst;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityLogicEffectGroup))]
    [UpdateAfter(typeof(ApplyDurationEffectPolicySystem))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct ApplyInfiniteEffectPolicySystem : ISystem
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
            new ApplyInfiniteEffectPolicyJob() { Ecb = ecb }.ScheduleParallel();
        }
    }

    [BurstCompile]
    [WithAll(typeof(InfiniteEffect))]
    [WithChangeFilter(typeof(InfiniteEffect))]
    [WithNone(typeof(IgnoreCleanupTag))]
    public partial struct ApplyInfiniteEffectPolicyJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;

        void Execute(Entity statModifierEntity, [EntityInQueryIndex] int entityInQueryIndex)
        {
            this.Ecb.AddComponent<IgnoreCleanupTag>(entityInQueryIndex, statModifierEntity);
        }
    }
}