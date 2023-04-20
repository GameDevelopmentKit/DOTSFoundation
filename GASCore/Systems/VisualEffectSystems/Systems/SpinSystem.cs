namespace GASCore.Systems.VisualEffectSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.VisualEffectSystems.Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [UpdateInGroup(typeof(GameAbilityFixedUpdateSystemGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct SpinSystem : ISystem
    { 
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;

            new SpinSystemJob { DeltaTime = deltaTime }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct SpinSystemJob : IJobEntity
    {
        public float DeltaTime;

        // need to be recalculated when HasComponent<PositionOffset> or HasComponent<RotationOffset>
        private void Execute(ref Spinnable data, ref LocalTransform transform)
        {
            // var angle = data.MaxSpinRate * math.cos(data.SpinRate * this.DeltaTime) * data.InvertSpin;
            data.SpinRate += data.SpinModificator * data.InvertSpin * this.DeltaTime;
            if (math.abs(data.SpinRate) > data.MaxSpinRate)
            {
                data.SpinRate   =  data.MaxSpinRate * 2 * data.InvertSpin - data.SpinRate;
                data.InvertSpin *= -1;
            }

            var angle = quaternion.RotateY(data.ClockWise * math.abs(math.radians(data.SpinRate)));
            transform.Rotation = math.mul(transform.Rotation, angle);
        }
    }
}