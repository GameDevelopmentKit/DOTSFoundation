namespace GASCore.Systems.LogicEffectSystems.Components
{
    using GASCore.Interfaces;
    using Unity.Entities;

    public struct TriggerAbilityComponent : IComponentData, IAbilityActionComponentConverter
    {
        public int  Slot;
        public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity) { ecb.AddComponent(index, entity, new TriggerAbilityComponent() { Slot = this.Slot }); }
    }
}