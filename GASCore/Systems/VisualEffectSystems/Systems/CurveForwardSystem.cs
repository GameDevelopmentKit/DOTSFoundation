namespace GASCore.Systems.VisualEffectSystems.Systems
{
    using DOTSCore.CommonSystems.Components;
    using Gameplay.View.ViewMono;
    using GASCore.Groups;
    using GASCore.Systems.VisualEffectSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [UpdateInGroup(typeof(AbilityVisualEffectGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct CurveForwardSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime    = SystemAPI.Time.DeltaTime;

            new CurveForwardJob()
            {
                DeltaTime = deltaTime,
            }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct CurveForwardJob : IJobEntity
    {
        
        public float                              DeltaTime;

        [NativeSetThreadIndex] private int threadId;

        private void Execute(Entity entity, [EntityIndexInQuery] int index, ref LocalTransform transform, ref CurveForward data)
        {
            var random = Random.CreateFromIndex((uint)this.threadId);

            // init destination, rotation
            if (data.Clockwise == 0)
            {
                data.Destination   = transform.Position + math.forward(transform.Rotation) * data.Distance;
                data.Clockwise     = random.NextBool() ? 1 : -1;
                transform.Rotation = math.mul(transform.Rotation, quaternion.RotateY(-data.RemainingRotateAngle * data.Clockwise));
            }

            // arrive at destination
            //gameObjectHybridLink.Value.GetComponent<TargetViewOfProjectile>().UpdateTargetPosition(data.Destination);
            if (math.distancesq(transform.Position, data.Destination) < .1f)
            {
                return;
            }

            // rotate toward target
            if (data.RemainingRotateAngle > 0f)
            {
                var rotateAngle = math.min(data.RotateSpeed * this.DeltaTime, data.RemainingRotateAngle);
                data.RemainingRotateAngle -= rotateAngle;
                transform.Rotation        =  math.mul(transform.Rotation, quaternion.RotateY(rotateAngle * data.Clockwise));
            }
            else
            {
                transform.Rotation = quaternion.LookRotation(data.Destination - transform.Position, math.up());
            }

            // move toward target
            transform.Position += math.forward(transform.Rotation) * data.MoveSpeed * this.DeltaTime;
        }
    }
}