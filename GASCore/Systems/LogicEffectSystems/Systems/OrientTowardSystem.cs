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
    public partial struct OrientTowardJob : IJobEntity
    {
        public float  DeltaTime;
        public float3 UpVector;
        void Execute(ref LocalTransform transform, in MovementDirection movementDirection, StatAspect statAspect)
        {
            if (movementDirection.Value.Equals(float3.zero) || !statAspect.HasStat(StatName.RotateSpeed)) return;
            var toRotation = quaternion.LookRotation(movementDirection.Value, this.UpVector);
            transform.Rotation = math.slerp(transform.Rotation, toRotation, statAspect.GetCurrentValue(StatName.RotateSpeed) * this.DeltaTime);
        }
    }

    [UpdateInGroup(typeof(GameAbilityFixedUpdateSystemGroup))]
    [BurstCompile]
    public partial struct OrientTowardSystem : ISystem
    {
        private float3 upVector;
        [BurstCompile]
        public void OnCreate(ref SystemState state) { this.upVector = new float3(0, 1, 0); }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            new OrientTowardJob()
            {
                DeltaTime = deltaTime,
                UpVector  = this.upVector
            }.ScheduleParallel();
        }
    }
}