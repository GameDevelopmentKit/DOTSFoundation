namespace GASCore.Systems.TargetDetectionSystems.Components
{
    using GASCore.Interfaces;
    using Unity.Entities;

    public struct TriggerOnInAbilityRange : IComponentData
    {
        public class _ : ITriggerConditionActionConverter
        {
            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity) { ecb.AddComponent(index, entity, new TriggerOnInAbilityRange()); }
        }
    }
}