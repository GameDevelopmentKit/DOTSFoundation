namespace GASCore.Systems.TargetDetectionSystems.Components.Filters
{
    using Unity.Entities;

    public struct FilterOutsideCastRange : IComponentData
    {
        public class Option : IFilterTargetConverter
        {
            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
            {
                ecb.AddComponent<FilterOutsideCastRange>(index, entity);
            }
        }
    }
}