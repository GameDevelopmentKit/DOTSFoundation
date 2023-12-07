namespace GASCore.Systems.LogicEffectSystems.Components
{
    using GASCore.Interfaces;
    using Unity.Entities;

    public struct AttachDeathTagComponent : IComponentData
    {
        public class _ : IAbilityActionComponentConverter
        {
            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity) { ecb.AddComponent<AttachDeathTagComponent>(index, entity); }
        }
    }

    public struct DeathTag : IComponentData, IEnableableComponent { }
}