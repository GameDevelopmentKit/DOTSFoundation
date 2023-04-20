using GASCore.Systems.LogicEffectSystems.Components;

namespace GASCore.Systems.EntityGeneratorSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.EntityGeneratorSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Transforms;

    [UpdateInGroup(typeof(GameAbilityFixedUpdateSystemGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct AttachPositionToAffectedTargetSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new PositionAttachJob
                {
                    LocalTransformLookup      = SystemAPI.GetComponentLookup<LocalTransform>(false),
                    ParentLookup              = SystemAPI.GetComponentLookup<Parent>(true),
                    PostTransformMatrixLookup = SystemAPI.GetComponentLookup<PostTransformMatrix>(true),
                    PositionOffsetLookup      = SystemAPI.GetComponentLookup<PositionOffset>(true)
                }
                .ScheduleParallel();
        }
    }

    [WithAll(typeof(AttachPositionToAffectedTarget))]
    [BurstCompile]
    public partial struct PositionAttachJob : IJobEntity
    {
        [NativeDisableParallelForRestriction] public ComponentLookup<LocalTransform> LocalTransformLookup;

        [ReadOnly] public ComponentLookup<Parent>              ParentLookup;
        [ReadOnly] public ComponentLookup<PostTransformMatrix> PostTransformMatrixLookup;
        [ReadOnly] public ComponentLookup<PositionOffset>      PositionOffsetLookup;

        private void Execute(in SourceComponent sourceComponent, in AffectedTargetComponent affectedTarget)
        {
            Helpers.ComputeWorldTransformMatrix(affectedTarget, out var affectedTargetWorldMat, ref this.LocalTransformLookup, ref this.ParentLookup, ref this.PostTransformMatrixLookup);

            var sourceTransform = LocalTransformLookup[sourceComponent];

            var affectedTargetWorldPos = affectedTargetWorldMat.Translation();
            sourceTransform.Position = this.ParentLookup.HasComponent(sourceComponent)
                ? this.LocalTransformLookup[this.ParentLookup[sourceComponent].Value].InverseTransformPoint(affectedTargetWorldPos)
                : affectedTargetWorldPos;
            if (PositionOffsetLookup.TryGetComponent(sourceComponent, out var positionOffset))
            {
                sourceTransform.Position += positionOffset.Value;
            }

            LocalTransformLookup[sourceComponent] = sourceTransform;
        }
    }
}