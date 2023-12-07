namespace GASCore.Systems.TimelineSystems.Components
{
    using GASCore.Interfaces;
    using Unity.Entities;

    public struct ForceCleanupActivatedAbilityTag : IComponentData
    {
        public class _ : ITimelineActionComponentConverter
        {
            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity) { ecb.AddComponent(index, entity, new ForceCleanupActivatedAbilityTag()); }
        }
    }
}