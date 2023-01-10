namespace GASCore.Systems.VisualEffectSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.CommonSystems.Components;
    using GASCore.Systems.VisualEffectSystems.Components;
    using Unity.Burst;
    using Unity.Collections.LowLevel.Unsafe;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [UpdateInGroup(typeof(AbilityVisualEffectGroup))]
    [UpdateAfter(typeof(SetupInitialPositionSystem))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct CurveForwardSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<AbilityPresentEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            var deltaTime    = SystemAPI.Time.DeltaTime;

            new CurveForwardJob()
            {
                Ecb       = ecb,
                DeltaTime = deltaTime,
            }.ScheduleParallel();
        }
    }

    [BurstCompile]
    [WithNone(typeof(ArrivedAtDestinationTag))]
    public partial struct CurveForwardJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;
        public float                              DeltaTime;

        [NativeSetThreadIndex] private int threadId;

        private void Execute(Entity entity, [EntityInQueryIndex] int index, ref Translation translation, ref Rotation rotation, ref CurveForward data)
        {
            var random = Random.CreateFromIndex((uint)this.threadId);

            // init destination, rotation
            if (data.Clockwise == 0)
            {
                data.Destination = translation.Value + math.forward(rotation.Value) * data.Distance;
                data.Clockwise   = random.NextBool() ? 1 : -1;
                rotation.Value   = math.mul(rotation.Value, quaternion.RotateY(-data.RemainingRotateAngle * data.Clockwise));
            }

            // arrive at destination
            if (math.distancesq(translation.Value, data.Destination) < .1f)
            {
                this.Ecb.AddComponent<ArrivedAtDestinationTag>(index, entity);
                return;
            }

            // rotate toward target
            if (data.RemainingRotateAngle > 0f)
            {
                var rotateAngle = math.min(data.RotateSpeed * this.DeltaTime, data.RemainingRotateAngle);
                data.RemainingRotateAngle -= rotateAngle;
                rotation.Value            =  math.mul(rotation.Value, quaternion.RotateY(rotateAngle * data.Clockwise));
            }
            else
            {
                rotation.Value = quaternion.LookRotation(data.Destination - translation.Value, math.up());
            }

            // move toward target
            translation.Value += math.forward(rotation.Value) * data.MoveSpeed * this.DeltaTime;
        }
    }
}