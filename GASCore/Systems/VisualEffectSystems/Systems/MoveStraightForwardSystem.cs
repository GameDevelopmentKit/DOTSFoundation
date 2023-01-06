namespace GASCore.Systems.VisualEffectSystems.Systems
{
    using DOTSCore.CommonSystems.Components;
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.VisualEffectSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [UpdateInGroup(typeof(AbilityVisualEffectGroup))]
    [UpdateAfter(typeof(SetupInitialPositionSystem))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct MoveStraightForwardSystem : ISystem
    {
        ComponentLookup<LocalToWorld> transformLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state) { }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<AbilityPresentEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            var lifeTimeJob = new MoveStraightForwardJob()
            {
                Ecb = ecb,
            };
            lifeTimeJob.ScheduleParallel();
        }
    }

    [BurstCompile]
    [WithAll(typeof(MoveStraightForward))]
    [WithNone(typeof(MovementDirection))]
    public partial struct MoveStraightForwardJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;
        void Execute(Entity entity, [EntityInQueryIndex] int entityInQueryIndex, in Rotation rotation)
        {
            this.Ecb.AddComponent(entityInQueryIndex, entity, new MovementDirection() { Value = math.forward(rotation.Value) });
        }
    }
}