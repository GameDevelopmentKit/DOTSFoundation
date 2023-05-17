namespace GASCore.Systems.TargetDetectionSystems.Systems.Filters
{
    using GASCore.Systems.LogicEffectSystems.Components;
    using GASCore.Systems.TargetDetectionSystems.Components;
    using GASCore.Systems.TargetDetectionSystems.Components.Filters;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;

    [UpdateInGroup(typeof(FilterTargetGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct FilterExcludeTagNameSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<FindTargetComponent>();
            state.RequireForUpdate<FilterExcludeTagName>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new FilterExcludeTagNameJob
            {
                TagLookup = SystemAPI.GetComponentLookup<TagComponent>(true),
            }.ScheduleParallel(state.Dependency);
        }
    }

    [WithAll(typeof(FindTargetComponent))]
    [WithAll(typeof(FilterExcludeTagName))]
    [BurstCompile]
    public partial struct FilterExcludeTagNameJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<TagComponent> TagLookup;

        private void Execute(
            ref DynamicBuffer<TargetableElement> targetables,
            in DynamicBuffer<FilterExcludeTagName> tagNames
        )
        {
            for (var i = 0; i < targetables.Length;)
            {
                if (!this.TagLookup.TryGetComponent(targetables[i], out var targetTag))
                {
                    continue;
                }

                var isValid = true;
                foreach (var tagName in tagNames)
                {
                    if (tagName == targetTag.Value)
                    {
                        isValid = false;
                        break;
                    }
                }

                if (!isValid)
                {
                    // match any tag
                    targetables.RemoveAtSwapBack(i);
                    continue;
                }

                ++i;
            }
        }
    }
}