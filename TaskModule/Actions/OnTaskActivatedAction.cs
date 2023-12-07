namespace TaskModule.Actions
{
    using Unity.Entities;

    public struct OnTaskActivatedAction : IBufferElementData
    {
        public Entity ActionEntityPrefab;
    }
}