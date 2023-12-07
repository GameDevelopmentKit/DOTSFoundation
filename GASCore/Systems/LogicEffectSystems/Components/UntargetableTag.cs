namespace GASCore.Systems.LogicEffectSystems.Components
{
    using GASCore.Interfaces;
    using Unity.Entities;

    public struct MarkUntargetable : IComponentData { }

    public class MarkUntargetableTagAuthoring : IAbilityActionComponentConverter
    {
        public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity) { ecb.AddComponent<MarkUntargetable>(index, entity); }
    }

    public struct WaitMarkUntargetableCleanup : ICleanupComponentData
    {
        public Entity AffectedTargetEntity;
    }

    public struct UntargetableTag : IComponentData, IEnableableComponent { }
}