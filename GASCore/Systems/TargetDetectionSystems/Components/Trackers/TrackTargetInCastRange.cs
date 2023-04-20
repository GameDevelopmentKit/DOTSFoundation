namespace GASCore.Systems.TargetDetectionSystems.Components.Trackers
{
    using GASCore.Systems.AbilityMainFlow.Components;
    using Unity.Entities;

    public struct TrackTargetInCastRange : IBufferElementData
    {
        public ActivatedStateEntityOwner Owner;
    }
}