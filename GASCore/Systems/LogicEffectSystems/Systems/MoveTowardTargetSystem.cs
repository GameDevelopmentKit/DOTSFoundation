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

    public struct MoveTowardTarget : IComponentData { }

    [BurstCompile]
    public partial struct UpdateMoveDirJob : IJobEntity
    {
        private void Execute(ref MovementDirection movementDirection, ref LocalTransform transform, in TargetPosition target)
        {
            if (math.distancesq(transform.Position, target.Value).IsZero(target.RadiusSq))
            {
                movementDirection.Value = float3.zero;
                if (target.RadiusSq <= 0.01f)
                {
                    transform.Position = target.Value;
                }
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

    [UpdateInGroup(typeof(GameAbilityFixedUpdateSystemGroup))]
    [BurstCompile]
    public partial struct MoveTowardTargetSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new ChaseTargetJob()
            {
                TransformLookup = SystemAPI.GetComponentLookup<LocalToWorld>(true)
            }.ScheduleParallel();

            new UpdateMoveDirJob().ScheduleParallel();
        }
    }


    [UpdateInGroup(typeof(GameAbilityBeginSimulationSystemGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct AddMoveDirComponentSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { state.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<TargetPosition>().WithNone<MovementDirection>().Build()); }
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            new AddMoveDirComponentJob() { Ecb = ecb }.ScheduleParallel();
        }

        [WithAll(typeof(TargetPosition), typeof(MoveTowardTarget))]
        [WithNone(typeof(MovementDirection))]
        [BurstCompile]
        public partial struct AddMoveDirComponentJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter Ecb;

            private void Execute(Entity entity, [EntityIndexInQuery] int index) { this.Ecb.AddComponent<MovementDirection>(index, entity); }
        }
    }
}