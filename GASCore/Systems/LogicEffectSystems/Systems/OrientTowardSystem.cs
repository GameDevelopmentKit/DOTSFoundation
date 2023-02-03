namespace GASCore.Systems.LogicEffectSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.LogicEffectSystems.Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [BurstCompile]
    public partial struct OrientTowardJob : IJobEntity
    {
        public float  DeltaTime;
        public float3 UpVector;
        void Execute(ref Rotation rot, in MovementDirection movementDirection, in RotationSpeed speed)
        {
            if (movementDirection.Value.Equals(float3.zero)) return;
            var toRotation        = quaternion.LookRotation(movementDirection.Value, this.UpVector);
            rot.Value = math.slerp(rot.Value, toRotation, speed.Value * this.DeltaTime);
        }
    }

    [UpdateInGroup(typeof(AbilityLogicEffectGroup))]
    [BurstCompile]
    public partial struct OrientTowardSystem : ISystem
    {
        private float3 upVector;
        [BurstCompile]
        public void OnCreate(ref SystemState state) { this.upVector = new float3(0, 1, 0); }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            new OrientTowardJob()
            {
                DeltaTime = deltaTime,
                UpVector =  this.upVector
            }.ScheduleParallel();
        }
    }
}