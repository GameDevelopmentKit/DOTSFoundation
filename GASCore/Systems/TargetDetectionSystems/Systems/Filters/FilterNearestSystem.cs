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
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<FindTargetComponent>();
            state.RequireForUpdate<FilterNearest>();
        }

        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new FilterNearestJob
            {
                WorldTransformLookup = SystemAPI.GetComponentLookup<LocalToWorld>(true),
            }.ScheduleParallel(state.Dependency);
        }
    }

    [WithAll(typeof(FindTargetComponent))]
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