namespace GASCore.Systems.TargetDetectionSystems.Systems.Filters
{
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.TargetDetectionSystems.Components;
    using GASCore.Systems.TargetDetectionSystems.Components.Filters;
    using GASCore.Systems.TargetDetectionSystems.Components.Trackers;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [UpdateInGroup(typeof(FilterTargetGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct FilterOutsideCastRangeSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<FindTargetTagComponent>();
            state.RequireForUpdate<FilterOutsideCastRange>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new FilterOutsideCastRangeJob
            {
                WorldTransformLookup = SystemAPI.GetComponentLookup<LocalToWorld>(true),
                CastRangeLookup      = SystemAPI.GetComponentLookup<CastRangeComponent>(true),
                TrackLookup          = SystemAPI.GetBufferLookup<TrackTargetInCastRange>(true),
            }.ScheduleParallel();
        }
    }

    [WithAll(typeof(FindTargetTagComponent))]
    [WithAll(typeof(FilterOutsideCastRange))]
    [BurstCompile]
    public partial struct FilterOutsideCastRangeJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<LocalToWorld>        WorldTransformLookup;
        [ReadOnly] public ComponentLookup<CastRangeComponent>  CastRangeLookup;
        [ReadOnly] public BufferLookup<TrackTargetInCastRange> TrackLookup;

        private void Execute(
            ref DynamicBuffer<TargetableElement> targetables,
            in CasterComponent caster,
            in ActivatedStateEntityOwner owner
        )
        {
            var castRange      = this.CastRangeLookup[owner];
            var casterPosition = this.WorldTransformLookup[caster].Position;
            for (var i = 0; i < targetables.Length;)
            {
                if (!this.TrackLookup.TryGetBuffer(targetables[i], out var tracks))
                {
                    // not tracked
                    targetables.RemoveAtSwapBack(i);
                    continue;
                }

                var isValid = false;
                foreach (var track in tracks)
                {
                    if (track.Owner.Value == owner.Value)
                    {
                        isValid = true;
                        break;
                    }
                }

                if (!isValid)
                {
                    // not tracked
                    targetables.RemoveAtSwapBack(i);
                    continue;
                }

                var distanceSq = math.distancesq(casterPosition, this.WorldTransformLookup[targetables[i]].Position);
                if (distanceSq <= castRange.ValueSqr)
                {
                    // inside cast range
                    targetables.RemoveAtSwapBack(i);
                    continue;
                }

                ++i;
            }
        }
    }
}