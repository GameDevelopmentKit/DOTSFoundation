namespace GASCore.Systems.TargetDetectionSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using GASCore.Systems.StatSystems.Components;
    using GASCore.Systems.TargetDetectionSystems.Components;
    using GASCore.Systems.TimelineSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [UpdateInGroup(typeof(AbilityTimelineGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct FindTargetInCastRangeSystem : ISystem
    {
        private EntityQuery                         entityQuery;
        private ComponentLookup<WorldTransform>     worldTransformLookup;
        private ComponentLookup<TagComponent>       tagLookup;
        private ComponentLookup<CastRangeComponent> castRangeLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            using var queryBuilder = new EntityQueryBuilder(Allocator.Temp).WithAll<TeamOwnerId, StatNameToIndex>();
            this.entityQuery          = state.GetEntityQuery(queryBuilder);
            this.worldTransformLookup = state.GetComponentLookup<WorldTransform>(true);
            this.tagLookup            = state.GetComponentLookup<TagComponent>(true);
            this.castRangeLookup      = state.GetComponentLookup<CastRangeComponent>(true);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            this.worldTransformLookup.Update(ref state);
            this.tagLookup.Update(ref state);
            this.castRangeLookup.Update(ref state);

            var entities = this.entityQuery.ToEntityListAsync(state.WorldUpdateAllocator, state.Dependency, out var queryJob);

            state.Dependency = new FindTargetInCastRangeJob
            {
                Entities             = entities,
                WorldTransformLookup = this.worldTransformLookup,
                TagLookup            = this.tagLookup,
                CastRangeLookup      = this.castRangeLookup,
            }.ScheduleParallel(queryJob);
        }
    }

    [WithChangeFilter(typeof(FindTargetInCastRangeComponent))]
    [BurstCompile]
    public partial struct FindTargetInCastRangeJob : IJobEntity
    {
        [ReadOnly] public NativeList<Entity>                  Entities;
        [ReadOnly] public ComponentLookup<WorldTransform>     WorldTransformLookup;
        [ReadOnly] public ComponentLookup<TagComponent>       TagLookup;
        [ReadOnly] public ComponentLookup<CastRangeComponent> CastRangeLookup;

        private bool CheckRange(in Entity e1, in Entity e2, in CastRangeComponent castRange)
        {
            return math.distancesq(this.WorldTransformLookup[e1].Position, this.WorldTransformLookup[e2].Position) <= castRange.Value * castRange.Value;
        }

        private bool CheckTag(in Entity e2, in FindTargetInCastRangeComponent data)
        {
            if (!data.IncludeTag) return true;
            if (!this.TagLookup.TryGetComponent(e2, out var tag)) return false;
            return tag == data.Tag;
        }

        private void Execute(ref DynamicBuffer<TargetableElement> targetables, ref DynamicBuffer<CompletedTriggerElement> completedTriggerBuffer, in FindTargetInCastRangeComponent data, CasterComponent caster, in ActivatedStateEntityOwner owner)
        {
            var castRange = this.CastRangeLookup[owner];
            foreach (var entity in this.Entities)
            {
                if (!this.CheckRange(caster, entity, castRange)) continue;
                if (!this.CheckTag(entity, data)) continue;
                targetables.Add(entity);
            }

            completedTriggerBuffer.Add(new CompletedTriggerElement() { Index = TypeManager.GetTypeIndex<FindTargetInCastRangeComponent>() });
        }
    }
}