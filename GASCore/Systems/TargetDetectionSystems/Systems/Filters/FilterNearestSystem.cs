namespace GASCore.Systems.TargetDetectionSystems.Systems.Filters
{
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.TargetDetectionSystems.Components;
    using GASCore.Systems.TargetDetectionSystems.Components.Filters;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [UpdateInGroup(typeof(FilterTargetGroup), OrderLast = true)]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct FilterNearestSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<FindTargetTagComponent>();
            state.RequireForUpdate<FilterNearest>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new FilterNearestJob
            {
                WorldTransformLookup = SystemAPI.GetComponentLookup<LocalToWorld>(true),
            }.ScheduleParallel();
        }
    }

    [WithAll(typeof(FindTargetTagComponent))]
    [WithAll(typeof(FilterNearest))]
    [BurstCompile]
    public partial struct FilterNearestJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<LocalToWorld> WorldTransformLookup;

        private void Execute(
            ref DynamicBuffer<TargetableElement> targetables,
            in FilterNearest param,
            in CasterComponent caster
        )
        {
            if (param.Strict && targetables.Length <= param.Amount)
            {
                targetables.Clear();
                return;
            }

            // Remove caster from targetables
            for (var index = 0; index < targetables.Length; index++)
            {
                var targetElement = targetables[index];
                if (caster.Value != targetElement) continue;
                targetables.RemoveAtSwapBack(index);
                break;
            }

            var maxTarget      = math.min(param.Amount, targetables.Length);
            var casterPosition = this.WorldTransformLookup[caster].Position;
            for (var startIndex = 0; startIndex < maxTarget; ++startIndex)
            {
                var nearestIndex      = startIndex;
                var nearestDistanceSq = math.distancesq(casterPosition, this.WorldTransformLookup[targetables[nearestIndex]].Position);
                for (var index = startIndex + 1; index < targetables.Length; ++index)
                {
                    var distanceSq = math.distancesq(casterPosition, this.WorldTransformLookup[targetables[index]].Position);
                    if (distanceSq < nearestDistanceSq)
                    {
                        nearestIndex      = index;
                        nearestDistanceSq = distanceSq;
                    }
                }

                (targetables[startIndex], targetables[nearestIndex]) = (targetables[nearestIndex], targetables[startIndex]);
            }

            targetables.ResizeUninitialized(maxTarget);
        }
    }
}