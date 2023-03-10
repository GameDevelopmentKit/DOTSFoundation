namespace GASCore.Systems.TargetDetectionSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.LogicEffectSystems.Components;
    using GASCore.Systems.TargetDetectionSystems.Components;
    using GASCore.Systems.TimelineSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityTimelineGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct FilterTargetableWithTagSystem : ISystem
    {
        private ComponentLookup<TagComponent> tagLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state) { this.tagLookup = state.GetComponentLookup<TagComponent>(true); }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            this.tagLookup.Update(ref state);

            new FilterTargetableWithTag() { TagLookup = this.tagLookup }.ScheduleParallel();
        }
    }

    [BurstCompile]
    [WithChangeFilter(typeof(TargetableElement))]
    public partial struct FilterTargetableWithTag : IJobEntity
    {
        [ReadOnly] public ComponentLookup<TagComponent> TagLookup;

        private void Execute(ref DynamicBuffer<TargetableElement> targets, ref DynamicBuffer<CompletedTriggerElement> completedTriggerBuffer, in IncludeTag includeTag)
        {
            for (var index = 0; index < targets.Length;)
            {
                var target = targets[index].Value;

                if (this.TagLookup.TryGetComponent(target, out var tag) && tag.Value.Equals(includeTag.Value))
                {
                    index++;
                }
                else
                {
                    targets.RemoveAtSwapBack(index);
                }
            }

            if (targets.Length > 0)
            {
                completedTriggerBuffer.Add(new CompletedTriggerElement() { Index = TypeManager.GetTypeIndex<IncludeTag>() });
            }
        }
    }
}