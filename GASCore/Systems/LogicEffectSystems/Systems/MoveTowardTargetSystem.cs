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
        private void Execute(ref MovementDirection movementDirection, in LocalToWorld transform, in TargetPosition target)
        {
            movementDirection.Value = math.distancesq(transform.Position, target.Value).IsZero(target.RadiusSq) ? float3.zero : math.normalize(target.Value - transform.Position);
        }
    }

    [BurstCompile]
    public partial struct ChaseTargetJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<LocalToWorld> TransformLookup;
        private void Execute(ref TargetPosition curTargetPosition, in Translation translation, in ChaseTargetEntity chaseTargetEntity)
        {
            if (this.TransformLookup.TryGetComponent(chaseTargetEntity.Value, out var targetTransform))
            {
                var resultPos = targetTransform.Position;

                if (chaseTargetEntity.LockAxis.x) resultPos.x = translation.Value.x;
                if (chaseTargetEntity.LockAxis.y) resultPos.y = translation.Value.y;
                if (chaseTargetEntity.LockAxis.z) resultPos.z = translation.Value.z;
                
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

        private void Execute(Entity entity, [EntityInQueryIndex] int index) { this.Ecb.AddComponent<MovementDirection>(index, entity); }
    }


    [UpdateInGroup(typeof(AbilityLogicEffectGroup))]
    [BurstCompile]
    public partial struct MoveTowardTargetSystem : ISystem
    {
        private ComponentLookup<LocalToWorld> localToWorldLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state) { this.localToWorldLookup = state.GetComponentLookup<LocalToWorld>(true); }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            this.localToWorldLookup.Update(ref state);

            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            new ChaseTargetJob()
            {
                TransformLookup = this.localToWorldLookup
            }.ScheduleParallel();

            new AddMoveDirComponentJob()
            {
                Ecb = ecb
            }.ScheduleParallel();
            new UpdateMoveDirJob().ScheduleParallel();
        }
    }
}