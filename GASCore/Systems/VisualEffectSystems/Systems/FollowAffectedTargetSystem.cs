namespace GASCore.Systems.VisualEffectSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using GASCore.Systems.VisualEffectSystems.Components;
    using Unity.Burst;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityVisualEffectGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct FollowAffectedTargetSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<AbilityPresentEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            new FollowAffectedTargetJob
            {
                Ecb = ecb
            }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct FollowAffectedTargetJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;

        private void Execute([EntityInQueryIndex] int entityInQueryIndex, in FollowAffectedTarget data, in SourceComponent source, in AffectedTargetComponent affectedTarget)
        {
            this.Ecb.AddComponent(entityInQueryIndex, source.Value, new ChaseTargetEntity()
            {
                Value    = affectedTarget.Value,
                LockAxis = data.LockAxis
            });
            this.Ecb.AddComponent(entityInQueryIndex, source.Value, new TargetPosition()
            {
                RadiusSq = data.Radius * data.Radius
            });
            this.Ecb.AddComponent(entityInQueryIndex, source.Value, new RotationSpeed() { Value = data.RotateSpeed });
        }
    }
}