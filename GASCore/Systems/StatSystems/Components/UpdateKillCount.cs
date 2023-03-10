namespace GASCore.Systems.StatSystems.Components
{
    using GASCore.Interfaces;
    using Unity.Entities;

    public struct UpdateKillCountTag : IComponentData
    {
    }

    public class UpdateKillCount : IAbilityActionComponentConverter
    {
        public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
        {
            ecb.AddComponent(index, entity, new UpdateKillCountTag());
            ecb.AddBuffer<ModifierAggregatorData>(index, entity);
        }
    }
}