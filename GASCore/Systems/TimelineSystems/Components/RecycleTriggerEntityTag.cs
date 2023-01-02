namespace GASCore.Systems.TimelineSystems.Components
{
    using GASCore.Interfaces;
    using Unity.Entities;

    public struct RecycleTriggerEntityTag : IComponentData
    {
        public class _ : ITimelineActionComponentConverter, IAbilityActivateConditionConverter
        {
            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity) { ecb.AddComponent<RecycleTriggerEntityTag>(index, entity); }
        }
    }
}