namespace TaskModule.TaskBase
{
    using Unity.Entities;

    public struct ActivatedTag : IComponentData, IEnableableComponent { }

    public struct CompletedTag : IComponentData, IEnableableComponent { }

    public struct AbandonedTag : IComponentData, IEnableableComponent { }

    public struct FailedTag : IComponentData, IEnableableComponent { }
}