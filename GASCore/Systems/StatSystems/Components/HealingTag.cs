namespace GASCore.Systems.StatSystems.Components
{
    using GASCore.Interfaces;
    using Unity.Entities;

    public struct HealingTag : IComponentData, IAbilityActionComponentConverter
    {
        public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity) { ecb.AddComponent<HealingTag>(index, entity); }
    }
}