namespace TaskModule.TaskBase
{
    using Unity.Entities;

    public struct TaskProgress : IComponentData
    {
        public float Value;
    }
}