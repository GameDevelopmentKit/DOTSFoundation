namespace TaskModule.Actions
{
    using Unity.Entities;

    public struct ActiveSiblingTaskOnComplete : IComponentData
    {
        public int TaskOrder;
    }
}