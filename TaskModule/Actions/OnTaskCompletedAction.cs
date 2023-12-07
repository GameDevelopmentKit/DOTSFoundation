namespace TaskModule.Actions
{
    using Unity.Entities;

    public struct OnTaskCompletedAction : IBufferElementData
    {
        public Entity ActionEntityPrefab;
    }
}