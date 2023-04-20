namespace GASCore.Systems.LogicEffectSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.LogicEffectSystems.Components;
    using GASCore.Systems.StatSystems.Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [BurstCompile]
    public partial struct MovementEntityInDirJob : IJobEntity
    {
        public float DeltaTime;
        void Execute(ref LocalTransform transform, in MovementDirection direction, in StatAspect statAspect)
        {
            if (direction.Value.Equals(float3.zero)) return;
            transform.Position +=  direction.Value * statAspect.GetCurrentValue(StatName.MovementSpeed) * this.DeltaTime;
        }
    }

    [UpdateInGroup(typeof(GameAbilityFixedUpdateSystemGroup))]
    [BurstCompile]
    public partial struct MovementInDirSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            new MovementEntityInDirJob() { DeltaTime = deltaTime }.ScheduleParallel();
        }
    }
}