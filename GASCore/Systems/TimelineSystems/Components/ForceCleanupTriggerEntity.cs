namespace GASCore.Systems.TimelineSystems.Components
{
    using GASCore.Interfaces;
    using Unity.Entities;

    public struct ForceCleanupTriggerEntity : IComponentData, ITimelineActionComponentConverter
    {
        public int  TriggerIndex;
        public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
        {
            ecb.AddComponent(index, entity, this);
        }
    }
}