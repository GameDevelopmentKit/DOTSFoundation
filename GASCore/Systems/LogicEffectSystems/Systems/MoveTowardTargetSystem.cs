namespace GASCore.Systems.LogicEffectSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.LogicEffectSystems.Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [BurstCompile]
    public partial struct UpdateMoveDirJob : IJobEntity
    {
        private void Execute(ref MovementDirection movementDirection, in LocalToWorld transform, in TargetPosition target)
        {
            var vectorToTarget = target.Value - transform.Position;
            // Normalize the vector to our target - this will be our movement direction
            movementDirection.Value = math.normalize(vectorToTarget);
        }
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
        public void OnUpdate(ref SystemState state) { new UpdateMoveDirJob().ScheduleParallel(); }
    }
}