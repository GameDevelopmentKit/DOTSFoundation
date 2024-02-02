namespace GASCore.Systems.TargetDetectionSystems.Components.Filters
{
    using Unity.Entities;

    public struct FilterDamagedTag : IComponentData
    {
        public class Option : IFilterTargetConverter
        {
            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
            {
                ecb.AddComponent<FilterDamagedTag>(index, entity);
                ecb.AddComponent<OverrideFindAllTargetTag>(index, entity);
            }
        }
    }
}