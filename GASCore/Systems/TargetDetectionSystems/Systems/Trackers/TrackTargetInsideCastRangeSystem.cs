namespace GASCore.Systems.TargetDetectionSystems.Systems.Trackers
{
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.TargetDetectionSystems.Components;
    using GASCore.Systems.TargetDetectionSystems.Components.Filters;
    using GASCore.Systems.TargetDetectionSystems.Components.Trackers;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;

    [UpdateInGroup(typeof(TrackTargetGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct TrackTargetInsideCastRangeSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<FindTargetComponent>();
            state.RequireForUpdate(SystemAPI.QueryBuilder().WithAny<FilterInsideCastRange, FilterOutsideCastRange>().Build());
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            var trackJob = new TrackTargetInsideCastRangeJob
            {
                Ecb          = ecb,
                TracksLookup = SystemAPI.GetBufferLookup<TrackTargetInCastRange>(),
            }.ScheduleParallel(state.Dependency);

            state.Dependency = new UntrackTargetOutsideCastRangeJob
            {
                Ecb = ecb,
            }.ScheduleParallel(trackJob);
        }
    }

    [WithAll(typeof(FindTargetComponent))]
    [WithAll(typeof(FilterInsideCastRange))]
    [BurstCompile]
    public partial struct TrackTargetInsideCastRangeJob : IJobEntity
    {
        public            EntityCommandBuffer.ParallelWriter   Ecb;
        [ReadOnly] public BufferLookup<TrackTargetInCastRange> TracksLookup;

        private void Execute(
            [EntityIndexInQuery] int index,
            in DynamicBuffer<TargetableElement> targetables,
            in FilterInsideCastRange param,
            in ActivatedStateEntityOwner owner
        )
        {
            if (!param.Track) return;
            foreach (var target in targetables)
            {
                if (!this.TracksLookup.HasBuffer(target))
                {
                    this.Ecb.AddBuffer<TrackTargetInCastRange>(index, target);
                }

                this.Ecb.AppendToBuffer(index, target, new TrackTargetInCastRange { Owner = owner });
            }
        }
    }

    [WithAll(typeof(FindTargetComponent))]
    [WithAll(typeof(FilterOutsideCastRange))]
    [BurstCompile]
    public partial struct UntrackTargetOutsideCastRangeJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;

        private void Execute(
            [EntityIndexInQuery] int index,
            in DynamicBuffer<TargetableElement> targetables,
            in ActivatedStateEntityOwner owner
        )
        {
            foreach (var target in targetables)
            {
                var tracks = this.Ecb.SetBuffer<TrackTargetInCastRange>(index, target);
                for (var i = 0; i < tracks.Length; ++i)
                {
                    if (tracks[i].Owner.Value == owner.Value)
                    {
                        tracks.RemoveAtSwapBack(i);
                        break;
                    }
                }
            }
        }
    }
}