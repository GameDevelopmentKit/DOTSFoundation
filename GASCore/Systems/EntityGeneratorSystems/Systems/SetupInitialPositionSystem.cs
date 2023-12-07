namespace GASCore.Systems.EntityGeneratorSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.EntityGeneratorSystems.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Transforms;
    using Random = Unity.Mathematics.Random;

    [UpdateInGroup(typeof(GameAbilityBeginSimulationSystemGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct SetupInitialPositionSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new CopyAffectedTargetTransformJob()
            {
                LocalTransformLookup      = SystemAPI.GetComponentLookup<LocalTransform>(false),
                ParentLookup              = SystemAPI.GetComponentLookup<Parent>(true),
                PostTransformMatrixLookup = SystemAPI.GetComponentLookup<PostTransformMatrix>(true),
                PositionOffsetLookup      = SystemAPI.GetComponentLookup<PositionOffset>(true)
            }.ScheduleParallel(state.Dependency);
            state.Dependency = new RandomPositionOffsetJob() { RandomSeed = (uint)SystemAPI.Time.ElapsedTime * 100000 }.ScheduleParallel(state.Dependency);
            state.Dependency = new PositionOffsetJob().ScheduleParallel(state.Dependency);
        }
    }

    [WithChangeFilter(typeof(RandomPositionOffset))]
    [BurstCompile]
    public partial struct RandomPositionOffsetJob : IJobEntity
    {
        public uint RandomSeed;
        private void Execute([EntityIndexInQuery] int index, in RandomPositionOffset randomPositionOffset, ref PositionOffset positionOffset)
        {
            var rnd = Random.CreateFromIndex((uint)(this.RandomSeed + index));
            positionOffset.Value = rnd.NextFloat3(randomPositionOffset.Min, randomPositionOffset.Max);
        }
    }

    [WithNone(typeof(AttachPositionToAffectedTarget))]
    [WithChangeFilter(typeof(PositionOffset))]
    [BurstCompile]
    public partial struct PositionOffsetJob : IJobEntity
    {
        private void Execute(in PositionOffset positionOffset, ref LocalTransform localTransform) { localTransform.Position += positionOffset.Value; }
    }


    [WithChangeFilter(typeof(CopyAffectedTargetTransform))]
    [BurstCompile]
    public partial struct CopyAffectedTargetTransformJob : IJobEntity
    {
        [NativeDisableParallelForRestriction] public ComponentLookup<LocalTransform> LocalTransformLookup;

        [ReadOnly] public ComponentLookup<Parent>              ParentLookup;
        [ReadOnly] public ComponentLookup<PostTransformMatrix> PostTransformMatrixLookup;
        [ReadOnly] public ComponentLookup<PositionOffset>      PositionOffsetLookup;

        private void Execute(Entity entity, in AffectedTargetComponent affectedTarget, in CopyAffectedTargetTransform copyAffectedTargetTransform)
        {
            TransformHelpers.ComputeWorldTransformMatrix(affectedTarget, out var affectedTargetWorldMat, ref this.LocalTransformLookup, ref this.ParentLookup, ref this.PostTransformMatrixLookup);

            var sourceTransform = LocalTransformLookup[entity];

            if (copyAffectedTargetTransform.Position)
            {
                var affectedTargetWorldPos = affectedTargetWorldMat.Translation();
                sourceTransform.Position = this.ParentLookup.HasComponent(entity)
                    ? this.LocalTransformLookup[this.ParentLookup[entity].Value].InverseTransformPoint(affectedTargetWorldPos)
                    : affectedTargetWorldPos;
                if (PositionOffsetLookup.TryGetComponent(entity, out var positionOffset))
                {
                    sourceTransform.Position += positionOffset.Value;
                }
            }

            if (copyAffectedTargetTransform.Rotation)
            {
                var affectedTargetWorldRos = affectedTargetWorldMat.Rotation();
                sourceTransform.Rotation = this.ParentLookup.HasComponent(entity)
                    ? this.LocalTransformLookup[this.ParentLookup[entity].Value].InverseTransformRotation(affectedTargetWorldRos)
                    : affectedTargetWorldRos;
            }

            LocalTransformLookup[entity] = sourceTransform;
        }
    }
}