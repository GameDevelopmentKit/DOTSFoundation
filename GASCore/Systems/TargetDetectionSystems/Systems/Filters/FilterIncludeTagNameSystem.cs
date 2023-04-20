namespace GASCore.Systems.TargetDetectionSystems.Systems.Filters
{
    using GASCore.Systems.LogicEffectSystems.Components;
    using GASCore.Systems.TargetDetectionSystems.Components;
    using GASCore.Systems.TargetDetectionSystems.Components.Filters;
    using GASCore.Systems.TimelineSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;

    [UpdateInGroup(typeof(FilterTargetGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct FilterIncludeTagNameSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<FindTargetComponent>();
            state.RequireForUpdate<FilterIncludeTagName>();
        }

        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new FilterIncludeTagNameJob
            {
                TagLookup = SystemAPI.GetComponentLookup<TagComponent>(true),
            }.ScheduleParallel(state.Dependency);
        }
    }

    [WithAll(typeof(FindTargetComponent))]
    [WithAll(typeof(FilterIncludeTagName))]
    [BurstCompile]
    public partial struct FilterIncludeTagNameJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<TagComponent> TagLookup;

        private void Execute(
            ref DynamicBuffer<TargetableElement> targetables,
            in DynamicBuffer<FilterIncludeTagName> tagNames
        )
        {
            for (var i = 0; i < targetables.Length;)
            {
                if (!this.TagLookup.TryGetComponent(targetables[i], out var targetTag))
                {
                    // no tag
                    targetables.RemoveAtSwapBack(i);
                    continue;
                }

                var isValid = false;
                foreach (var tagName in tagNames)
                {
                    if (tagName == targetTag.Value)
                    {
                        isValid = true;
                        break;
                    }
                }

                if (!isValid)
                {
                    // no matching tag
                    targetables.RemoveAtSwapBack(i);
                    continue;
                }

                ++i;
            }
        }
    }
}