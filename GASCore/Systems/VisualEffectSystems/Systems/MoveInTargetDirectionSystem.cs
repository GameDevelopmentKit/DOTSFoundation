namespace GASCore.Systems.VisualEffectSystems.Systems
{
    using DOTSCore.CommonSystems.Components;
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.VisualEffectSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [UpdateInGroup(typeof(AbilityVisualEffectGroup))]
    [UpdateAfter(typeof(SetupInitialPositionSystem))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct MoveInTargetDirectionSystem : ISystem
    {
        ComponentLookup<LocalToWorld> movementDirectionLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state) { movementDirectionLookup = state.GetComponentLookup<LocalToWorld>(true); }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<AbilityPresentEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            movementDirectionLookup.Update(ref state);
            var lifeTimeJob = new MoveInTargetDirectionJob()
            {
                Ecb                     = ecb,
                MovementDirectionLookup = movementDirectionLookup
            };
            lifeTimeJob.ScheduleParallel();
        }
    }

    [BurstCompile]
    [WithAll(typeof(MoveInAffectedTargetDir))]
    [WithNone(typeof(MovementDirection))]
    public partial struct MoveInTargetDirectionJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<LocalToWorld>      MovementDirectionLookup;
        public            EntityCommandBuffer.ParallelWriter Ecb;
        void Execute(Entity entity, [EntityInQueryIndex] int entityInQueryIndex, ref Rotation rotation, in MoveInAffectedTargetDir moveInAffectedTargetDir, in AffectedTargetComponent affectedTarget)
        {
            if (this.MovementDirectionLookup.TryGetComponent(affectedTarget.Value, out var moveDir))
            {
                var dir = moveInAffectedTargetDir.IsReverse ? moveDir.Forward * -1 : moveDir.Forward;
                this.Ecb.AddComponent(entityInQueryIndex, entity, new MovementDirection() { Value = dir});
                rotation.Value = quaternion.LookRotation(dir, moveDir.Up);
            }
        }
    }
}