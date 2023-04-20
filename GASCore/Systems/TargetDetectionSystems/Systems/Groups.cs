namespace GASCore.Systems.TargetDetectionSystems.Systems
{
    using GASCore.Groups;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityTimelineGroup))]
    [UpdateBefore(typeof(FilterTargetGroup))]
    public partial class FindTargetGroup : ComponentSystemGroup
    {
    }

    [UpdateInGroup(typeof(AbilityTimelineGroup))]
    [UpdateAfter(typeof(FindTargetGroup))]
    [UpdateBefore(typeof(TrackTargetGroup))]
    public partial class FilterTargetGroup : ComponentSystemGroup
    {
    }


    [UpdateInGroup(typeof(AbilityTimelineGroup))]
    [UpdateAfter(typeof(FilterTargetGroup))]
    [UpdateBefore(typeof(FinalizeFindTargetGroup))]
    public partial class TrackTargetGroup : ComponentSystemGroup
    {
    }

    [UpdateInGroup(typeof(AbilityTimelineGroup))]
    [UpdateAfter(typeof(TrackTargetGroup))]
    public partial class FinalizeFindTargetGroup : ComponentSystemGroup
    {
    }
}