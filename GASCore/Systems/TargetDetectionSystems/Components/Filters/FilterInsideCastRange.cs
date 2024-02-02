namespace GASCore.Systems.TargetDetectionSystems.Components.Filters
{
    using Unity.Entities;

    public struct FilterInsideCastRange : IComponentData
    {
        public bool Track;

        public class Option : IFilterTargetConverter
        {
            public bool Track = false;

            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
            {
                ecb.AddComponent(index, entity, new FilterInsideCastRange
                {
                    Track = this.Track,
                });
            }
        }
    }
}