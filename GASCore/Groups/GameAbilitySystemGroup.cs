namespace GASCore.Groups
{
    using Unity.Entities;
    using Unity.Transforms;

    #region GameAbilityInitializeSystemGroup

    [UpdateInGroup(typeof(InitializationSystemGroup), OrderFirst = true)]
    [UpdateBefore(typeof(AbilityCleanupSystemGroup))]
    public partial class GameAbilityInitializeSystemGroup { }

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class AbilityMainFlowGroup  { }

    #endregion

    #region GameAbilitySimulationSystemGroup

    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
    [UpdateBefore(typeof(TransformSystemGroup))]
    public partial class GameAbilityBeginSimulationSystemGroup { }
    
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial class GameAbilityFixedUpdateSystemGroup { }

    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial class GameAbilityLateSimulationSystemGroup { }

    [UpdateInGroup(typeof(GameAbilityLateSimulationSystemGroup))]
    public partial class AbilityTimelineGroup : ComponentSystemGroup { }

    [UpdateInGroup(typeof(GameAbilityLateSimulationSystemGroup))]
    [UpdateAfter(typeof(AbilityTimelineGroup))]
    public partial class AbilityCommonSystemGroup : ComponentSystemGroup { }

    [UpdateInGroup(typeof(GameAbilityLateSimulationSystemGroup))]
    [UpdateAfter(typeof(AbilityCommonSystemGroup))]
    public partial class AbilityLogicEffectGroup : ComponentSystemGroup { }

    #endregion

    #region GameAbilityPresentSystemGroup

    [UpdateInGroup(typeof(PresentationSystemGroup), OrderFirst = true)]
    [UpdateBefore(typeof(BeginPresentationEntityCommandBufferSystem))]
    public partial class AbilityVisualEffectGroup  { }

    [UpdateInGroup(typeof(InitializationSystemGroup), OrderFirst = true)]
    [UpdateBefore(typeof(BeginInitializationEntityCommandBufferSystem))]
    public partial class AbilityCleanupSystemGroup { }
    

    #endregion
}