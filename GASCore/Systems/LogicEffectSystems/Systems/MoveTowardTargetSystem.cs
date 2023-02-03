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
            var vectorToTarget = target.Value - transform.Position;
            movementDirection.Value = vectorToTarget.IsZero(0.05f)
                ? float3.zero
                :
                // Normalize the vector to our target - this will be our movement direction
                math.normalize(vectorToTarget);
        }
    }

    [BurstCompile]
    public partial struct ChaseTargetJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<LocalToWorld> TransformLookup;
        private void Execute(ref TargetPosition targetPosition, in ChaseTargetEntity chaseTargetEntity)
        {
            if (this.TransformLookup.TryGetComponent(chaseTargetEntity.Value, out var transform))
            {
                targetPosition.Value = transform.Position;
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