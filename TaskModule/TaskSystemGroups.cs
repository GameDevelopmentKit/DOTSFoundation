namespace TaskModule
{
    using Unity.Entities;

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class TaskInitializeSystemGroup : ComponentSystemGroup { }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class TaskSimulationSystemGroup : ComponentSystemGroup { }

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class TaskPresentationSystemGroup : ComponentSystemGroup { }
}