namespace GASCore.Systems.TargetDetectionSystems.Components.Filters
{
    using Unity.Entities;

    public struct FilterNearest : IComponentData
    {
        public int  Amount;
        public bool Strict;

        public class Option : FindTargetAuthoring.IOptionConverter
        {
            public int  Amount = 1;
            public bool Strict = false;

            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
            {
                ecb.AddComponent(index, entity, new FilterNearest
                {
                    Amount = this.Amount,
                    Strict = this.Strict,
                });
            }
        }
    }
}