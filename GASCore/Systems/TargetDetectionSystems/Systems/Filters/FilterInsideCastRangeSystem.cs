namespace GASCore.Systems.TargetDetectionSystems.Systems.Filters
{
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.TargetDetectionSystems.Components;
    using GASCore.Systems.TargetDetectionSystems.Components.Filters;
    using GASCore.Systems.TargetDetectionSystems.Components.Trackers;
    using GASCore.Systems.TimelineSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [UpdateInGroup(typeof(FilterTargetGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct FilterInsideCastRangeSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<FindTargetComponent>();
            state.RequireForUpdate<FilterInsideCastRange>();
        }

        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new FilterInsideCastRangeJob
            {
                WorldTransformLookup = SystemAPI.GetComponentLookup<LocalToWorld>(true),
                CastRangeLookup      = SystemAPI.GetComponentLookup<CastRangeComponent>(true),
                TrackLookup          = SystemAPI.GetBufferLookup<TrackTargetInCastRange>(true),
            }.ScheduleParallel(state.Dependency);
        }
    }

    [WithAll(typeof(FindTargetComponent))]
    [WithAll(typeof(FilterInsideCastRange))]
    [BurstCompile]
    public partial struct FilterInsideCastRangeJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<LocalToWorld>        WorldTransformLookup;
        [ReadOnly] public ComponentLookup<CastRangeComponent>  CastRangeLookup;
        [ReadOnly] public BufferLookup<TrackTargetInCastRange> TrackLookup;

        private void Execute(
            ref DynamicBuffer<TargetableElement> targetables,
            in FilterInsideCastRange param,
            in CasterComponent caster,
            in ActivatedStateEntityOwner owner
        )
        {
            var castRange      = this.CastRangeLookup[owner];
            var casterPosition = this.WorldTransformLookup[caster].Position;
            for (var i = 0; i < targetables.Length;)
            {
                if (param.Track && this.TrackLookup.TryGetBuffer(targetables[i], out var tracks))
                {
                    var isValid = true;
                    foreach (var track in tracks)
                    {
                        if (track.Owner.Value == owner.Value)
                        {
                            isValid = false;
                            break;
                        }
                    }

                    if (!isValid)
                    {
                        // already inside cast range
                        targetables.RemoveAtSwapBack(i);
                        continue;
                    }
                }

                var distanceSq = math.distancesq(casterPosition, this.WorldTransformLookup[targetables[i]].Position);
                if (distanceSq > castRange.ValueSqr)
                {
                    // out of cast range
                    targetables.RemoveAtSwapBack(i);
                    continue;
                }

                ++i;
            }
        }
    }
}