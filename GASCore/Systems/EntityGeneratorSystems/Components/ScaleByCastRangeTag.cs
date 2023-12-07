namespace GASCore.Systems.EntityGeneratorSystems.Components
{
    using GASCore.Interfaces;
    using Unity.Entities;

    public struct ScaleByCastRangeTag : IComponentData { }

    public class SetScaleByCastRange : IAbilityActionComponentConverter
    {
        public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity) { ecb.AddComponent<ScaleByCastRangeTag>(index, entity); }
    }
}