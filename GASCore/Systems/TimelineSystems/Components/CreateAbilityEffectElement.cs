namespace GASCore.Systems.TimelineSystems.Components
{
    using System.Collections.Generic;
    using GASCore.Interfaces;
    using Unity.Collections;
    using Unity.Entities;

    public struct CreateAbilityEffectElement : IBufferElementData
    {
        public FixedString64Bytes EffectId;

        public class _ : ITimelineActionComponentConverter
        {
            public List<string> AbilityEffectIds;

            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
            {
                var abilityCostBuffer = ecb.AddBuffer<CreateAbilityEffectElement>(index, entity);

                foreach (var abilityEffectId in this.AbilityEffectIds)
                {
                    abilityCostBuffer.Add(new CreateAbilityEffectElement() { EffectId = abilityEffectId });
                }
                
                ecb.AddComponent<WaitingCreateEffect>(index, entity);
            }
        }
    }

    public struct WaitingCreateEffect : IComponentData, IEnableableComponent { }
}