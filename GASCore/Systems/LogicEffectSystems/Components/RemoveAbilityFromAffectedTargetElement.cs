namespace GASCore.Systems.LogicEffectSystems.Components
{
    using System.Collections.Generic;
    using GASCore.Interfaces;
    using Unity.Collections;
    using Unity.Entities;

    public struct RemoveAbilityFromAffectedTargetElement : IBufferElementData
    {
        public FixedString64Bytes AbilityId;
    }

    public class RemoveAbilities : IAbilityActionComponentConverter
    {
        public List<string> AbilityIds = new();

        public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
        {
            var bufferElement = ecb.AddBuffer<RemoveAbilityFromAffectedTargetElement>(index, entity);

            foreach (var abilityInfo in this.AbilityIds)
            {
                bufferElement.Add(new RemoveAbilityFromAffectedTargetElement()
                {
                    AbilityId = abilityInfo,
                });
            }
        }
    }
}