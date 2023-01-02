namespace GASCore.Systems.CommonSystems.Components
{
    using GASCore.Interfaces;
    using Unity.Entities;

    public struct LifeTime : IComponentData
    {
        public float Value;
        
        public class _: IAbilityActionComponentConverter, ITimelineActionComponentConverter
        {
            public float Value;
            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
            {
                ecb.AddComponent(index, entity, new LifeTime() { Value = this.Value });
            }
        }
    }
}