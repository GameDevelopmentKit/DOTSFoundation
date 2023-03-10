namespace GASCore.Systems.LogicEffectSystems.Systems
{
    using DOTSCore.Extension;
    using GASCore.Groups;
    using GASCore.Systems.LogicEffectSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [BurstCompile]
    public partial struct UpdateMoveDirJob : IJobEntity
    {
        private void Execute(ref MovementDirection movementDirection, ref LocalTransform transform, in TargetPosition target)
        {
            if (math.distancesq(transform.Position, target.Value).IsZero(target.RadiusSq))
            {
                movementDirection.Value = float3.zero;
                if (target.RadiusSq <= 0.01f) transform.Position = target.Value;
            }
            else
                movementDirection.Value = math.normalize(target.Value - transform.Position);
        }
    }

    [BurstCompile]
    public partial struct ChaseTargetJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<LocalToWorld> TransformLookup;
        private void Execute(ref TargetPosition curTargetPosition, in LocalTransform translation, in ChaseTargetEntity chaseTargetEntity)
        {
            if (this.TransformLookup.TryGetComponent(chaseTargetEntity.Value, out var targetTransform))
            {
                var resultPos = targetTransform.Position;

                if (chaseTargetEntity.LockAxis.x) resultPos.x = translation.Position.x;
                if (chaseTargetEntity.LockAxis.y) resultPos.y = translation.Position.y;
                if (chaseTargetEntity.LockAxis.z) resultPos.z = translation.Position.z;

                curTargetPosition.Value = resultPos;
            }
        }
    }

    [WithAll(typeof(TargetPosition))]
    [WithNone(typeof(MovementDirection))]
    [BurstCompile]
    public partial struct AddMoveDirComponentJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;

        private void Execute(Entity entity, [EntityIndexInQuery] int index) { this.Ecb.AddComponent<MovementDirection>(index, entity); }
    }


    [UpdateInGroup(typeof(AbilityLogicEffectGroup))]
    [BurstCompile]
    public partial struct MoveTowardTargetSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            new ChaseTargetJob()
            {
                TransformLookup = SystemAPI.GetComponentLookup<LocalToWorld>(true)
            }.ScheduleParallel();

            new AddMoveDirComponentJob()
            {
                Ecb = ecb
            }.ScheduleParallel();
            new UpdateMoveDirJob().ScheduleParallel();
        }
    }
}