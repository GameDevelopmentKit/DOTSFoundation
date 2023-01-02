namespace GASCore.Systems.LogicEffectSystems.Components
{
    using System.Collections.Generic;
    using GASCore.Blueprints;
    using GASCore.Interfaces;
    using GASCore.Systems.AbilityMainFlow.Components;
    using Unity.Entities;

    public class TargetTypeElementAuthoring : ITimelineActionComponentConverter
    {
        public List<TargetType> Targets;
        public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
        {
            var targetBuffer = ecb.AddBuffer<TargetTypeElement>(index, entity);
            foreach (var target in this.Targets)
            {
                targetBuffer.Add(new TargetTypeElement(){Value = target});
            }
        }
    }
}