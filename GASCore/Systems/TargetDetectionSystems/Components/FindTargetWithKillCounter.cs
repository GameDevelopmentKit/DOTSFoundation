namespace GASCore.Systems.TargetDetectionSystems.Components
{
    using GASCore.Interfaces;
    using Unity.Entities;

    public struct TriggerKillCountTag : IComponentData
    {
    }

    public class FindTargetWithKillCounter : ITriggerConditionActionConverter
    {
        public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
        {
            ecb.AddComponent(index, entity, new TriggerKillCountTag());
            ecb.AddBuffer<TargetWithStatElement>(index, entity);
        }
    }
}