namespace GASCore.Systems.TargetDetectionSystems.Components.Filters
{
    using GASCore.Blueprints;
    using Unity.Entities;

    public struct FilterTeam : IComponentData, IFilterTargetConverter
    {
        public TargetType Value;

        public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity) { ecb.AddComponent<FilterTeam>(index, entity, new FilterTeam() { Value = this.Value }); }
    }
}