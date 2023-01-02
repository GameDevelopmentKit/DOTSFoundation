namespace GASCore.Systems.LogicEffectSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using Unity.Burst;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityCleanupSystemGroup), OrderLast = true)]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct DestroyTargetSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            var setEndTimeTriggerAfterSecondJob = new DestroyTargetJob()
            {
                Ecb = ecb,
            };
            setEndTimeTriggerAfterSecondJob.ScheduleParallel();
        }
    }

    [BurstCompile]
    [WithChangeFilter(typeof(DestroyTargetTag))]
    [WithAll(typeof(DestroyTargetTag))]
    public partial struct DestroyTargetJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;
        void Execute([EntityInQueryIndex] int entityInQueryIndex, in AffectedTargetComponent affectedTarget) { this.Ecb.DestroyEntity(entityInQueryIndex, affectedTarget.Value); }
    }
}