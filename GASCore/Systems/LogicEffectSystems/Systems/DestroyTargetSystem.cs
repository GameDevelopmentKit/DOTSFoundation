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
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<DestroyTargetTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            new DestroyTargetJob() { Ecb = ecb}.ScheduleParallel();
        }
    }

    [BurstCompile]
    [WithAll(typeof(DestroyTargetTag))]
    public partial struct DestroyTargetJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;
        void Execute([EntityIndexInQuery] int entityInQueryIndex, in AffectedTargetComponent affectedTarget) { this.Ecb.DestroyEntity(entityInQueryIndex, affectedTarget.Value); }
    }
}