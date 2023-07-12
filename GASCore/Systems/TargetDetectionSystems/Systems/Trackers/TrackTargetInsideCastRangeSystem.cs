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
            state.RequireForUpdate<FilterOutsideCastRange>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

            ecb.ShouldPlayback = false;
            var trackJob = new TrackTargetInsideCastRangeJob
            {
                Ecb          = ecb,
                TracksLookup = SystemAPI.GetBufferLookup<TrackTargetInCastRange>(),
            }.Schedule(state.Dependency);

            state.Dependency = new UntrackTargetOutsideCastRangeJob
            {
                Ecb = ecb,
            }.Schedule(trackJob);
        }
    }

    [WithAll(typeof(FindTargetComponent))]
    [WithAll(typeof(FilterInsideCastRange))]
    [BurstCompile]
    public partial struct TrackTargetInsideCastRangeJob : IJobEntity
    {
        public            EntityCommandBuffer   Ecb;
        [ReadOnly] public BufferLookup<TrackTargetInCastRange> TracksLookup;

        private void Execute(
            in DynamicBuffer<TargetableElement> targetables,
            in FilterInsideCastRange param,
            in ActivatedStateEntityOwner owner
        )
        {
            if (!param.Track) return;
            if(targetables.IsEmpty) return;
            this.Ecb.ShouldPlayback = true;
            foreach (var target in targetables)
            {
                if (!this.TracksLookup.HasBuffer(target))
                {
                    this.Ecb.AddBuffer<TrackTargetInCastRange>(target);
                }

                this.Ecb.AppendToBuffer(target, new TrackTargetInCastRange { Owner = owner });
            }
        }
    }

    [WithAll(typeof(FindTargetComponent))]
    [WithAll(typeof(FilterOutsideCastRange))]
    [BurstCompile]
    public partial struct UntrackTargetOutsideCastRangeJob : IJobEntity
    {
        public EntityCommandBuffer Ecb;

        private void Execute(
            in DynamicBuffer<TargetableElement> targetables,
            in ActivatedStateEntityOwner owner
        )
        {
            if(targetables.IsEmpty) return;
            this.Ecb.ShouldPlayback = true;
            foreach (var target in targetables)
            {
                var tracks = this.Ecb.SetBuffer<TrackTargetInCastRange>(target);
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