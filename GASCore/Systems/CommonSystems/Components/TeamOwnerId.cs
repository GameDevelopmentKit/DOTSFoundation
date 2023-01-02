namespace GASCore.Systems.CommonSystems.Components
{
    using GASCore.Interfaces;
    using Unity.Entities;

    public struct TeamOwnerId : IComponentData
    {
        public int Value;
        
        public class _: IAbilityActionComponentConverter
        {
            public int Value;
            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
            {
                ecb.AddComponent(index, entity, new TeamOwnerId() { Value = this.Value });
            }
        }
    }
}