namespace GASCore.Systems.EntityGeneratorSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.EntityGeneratorSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Transforms;

    [UpdateInGroup(typeof(GameAbilityBeginSimulationSystemGroup))]
    [UpdateAfter(typeof(SetupInitialPositionSystem))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct AttachPositionToAffectedTargetSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new PositionAttachWithPositionOffsetJob { WorldLookup = SystemAPI.GetComponentLookup<LocalToWorld>(true) }.ScheduleParallel();
            new PositionAttachJob { WorldLookup                   = SystemAPI.GetComponentLookup<LocalToWorld>(true) }.ScheduleParallel();
        }
    }


    [WithAll(typeof(AttachPositionToAffectedTarget))]
    [BurstCompile]
    public partial struct PositionAttachWithPositionOffsetJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<LocalToWorld> WorldLookup;

        private void Execute(in AffectedTargetComponent affectedTarget, ref TransformAspect transformAspect, in PositionOffset positionOffset)
        {
            transformAspect.WorldPosition = this.WorldLookup[affectedTarget.Value].Position + positionOffset.Value;
        }
    }

    [WithAll(typeof(AttachPositionToAffectedTarget))]
    [WithNone(typeof(PositionOffset))]
    [BurstCompile]
    public partial struct PositionAttachJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<LocalToWorld> WorldLookup;

        private void Execute(in AffectedTargetComponent affectedTarget, ref TransformAspect transformAspect) { transformAspect.WorldPosition = this.WorldLookup[affectedTarget.Value].Position; }
    }
}