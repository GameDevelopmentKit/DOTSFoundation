namespace TaskModule.TaskBase
{
    using Unity.Entities;

    public struct TaskContainerSetting : IComponentData
    {
        public int RequireOptionalAmount;
    }
    
    public struct SubTaskEntity : IBufferElementData
    {
        public Entity Value;
    }
    
    public struct ContainerOwner : IComponentData
    {
        public Entity Value;
    }
    
    public struct OnSubTaskElementCompleted : IComponentData, IEnableableComponent{}
}