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
    public partial struct RotateMovementDirectionSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<RotateMovementDirection>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<BeginPresentationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            var random       = new Random((uint)(SystemAPI.Time.ElapsedTime * 100000));
            var deltaTime    = SystemAPI.Time.DeltaTime;

            state.Dependency = new RotateMovementDirectionJob
            {
                Ecb       = ecb,
                Random    = random,
                DeltaTime = deltaTime,
            }.ScheduleParallel(state.Dependency);
        }
    }

    [BurstCompile]
    public partial struct RotateMovementDirectionJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;
        public Random                             Random;
        public float                              DeltaTime;

        private void Execute(
            Entity entity,
            [EntityIndexInQuery] int index,
            ref MovementDirection movementDirection,
            ref RotateMovementDirection rotateMovementDirection,
            in LocalToWorld worldTransform
        )
        {
            if (rotateMovementDirection.RotateDirection == 0)
            {
                rotateMovementDirection.RotateDirection = this.Random.NextBool() ? 1 : -1;
            }

            rotateMovementDirection.RotateTimer -= this.DeltaTime;
            if (rotateMovementDirection.RotateTimer > 0) return;
            rotateMovementDirection.RotateTimer += rotateMovementDirection.RotateInterval;
            movementDirection.Value = math.forward(
                math.mul(
                    quaternion.LookRotation(movementDirection.Value, worldTransform.Up),
                    quaternion.RotateY(rotateMovementDirection.RotateSpeed * rotateMovementDirection.RotateDirection)
                )
            );

            if (--rotateMovementDirection.RotateCount == 0)
            {
                this.Ecb.SetComponentEnabled<RotateMovementDirection>(index, entity, false);
            }
        }
    }
}