using Unity.Physics.Systems;

namespace GASCore.Groups
{
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Unity.Entities;
    using Unity.Transforms;

    #region GameAbilityInitializeSystemGroup

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class GameAbilityInitializeSystemGroup { }

    [UpdateInGroup(typeof(GameAbilityInitializeSystemGroup))]
    public partial class AbilityMainFlowGroup : ComponentSystemGroup { }

    #endregion

    #region GameAbilitySimulationSystemGroup

    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
    [UpdateBefore(typeof(TransformSystemGroup))]
    public partial class GameAbilityBeginSimulationSystemGroup { }
    
    [UpdateInGroup(typeof(AfterPhysicsSystemGroup))]
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

    [UpdateInGroup(typeof(PresentationSystemGroup), OrderLast = true)]
    public partial class AbilityCleanupSystemGroup { }
    

    #endregion
}