namespace TaskModule.TaskBase
{
    using Unity.Entities;

    public struct AutoActiveOnStartTag : IComponentData { }

    public struct ActivatedTag : IComponentData, IEnableableComponent { }

    public struct CompletedTag : IComponentData, IEnableableComponent { }

    public struct AbandonedTag : IComponentData, IEnableableComponent { }

    public struct FailedTag : IComponentData, IEnableableComponent { }
}