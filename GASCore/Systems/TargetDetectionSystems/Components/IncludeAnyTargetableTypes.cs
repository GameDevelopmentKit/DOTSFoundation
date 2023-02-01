namespace GASCore.Systems.TargetDetectionSystems.Components
{
    using System.Collections.Generic;
    using GASCore.Blueprints;
    using GASCore.Interfaces;
    using GASCore.Systems.AbilityMainFlow.Components;
    using Unity.Entities;

    public class IncludeAnyTargetableTypes : ITriggerConditionActionConverter
    {
        public List<TargetType> Types;
        public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
        {
            var targetBuffer = ecb.AddBuffer<TargetTypeElement>(index, entity);
            foreach (var target in this.Types)
            {
                targetBuffer.Add(new TargetTypeElement() { Value = target });
            }
        }
    }
}