namespace GASCore.Systems.LogicEffectSystems.Components
{
    using Unity.Entities;

    public struct NoneStackingComponent : IComponentData
    {
        public bool IsRefreshDuration;
        public bool IsRefreshPeriod;
    }

    public struct StackingComponent : IComponentData
    {
        public StackingType StackingType;
        public bool         IsRefreshDuration;
        public bool         IsRefreshPeriod;

        public int                 StackLimitCount;
        public StackExpirationType StackExpirationPolicy;
    }

    public enum StackingType
    {
        None,
        AggregateBySource,
        AggregateByTarget
    }

    public enum StackExpirationType
    {
        ClearEntireStack,
        RemoveSingleStackAndRefreshDuration,
        RefreshDuration
    }
}