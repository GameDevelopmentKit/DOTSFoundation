namespace GASCore.Systems.VisualEffectSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.LogicEffectSystems.Components;
    using GASCore.Systems.VisualEffectSystems.Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [UpdateInGroup(typeof(AbilityVisualEffectGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct MoveStraightForwardSystem : ISystem
    {
        ComponentLookup<LocalToWorld> transformLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state) { state.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<MoveStraightForward>().WithNone<MovementDirection>().Build()); }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<BeginPresentationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            new MoveStraightForwardJob() { Ecb = ecb }.ScheduleParallel();
        }
    }

    [BurstCompile]
    [WithAll(typeof(MoveStraightForward))]
    [WithNone(typeof(MovementDirection))]
    public partial struct MoveStraightForwardJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;
        void Execute(Entity entity, [EntityIndexInQuery] int entityInQueryIndex, in LocalTransform transform)
        {
            this.Ecb.AddComponent(entityInQueryIndex, entity, new MovementDirection() { Value = math.forward(transform.Rotation) });
        }
    }
}