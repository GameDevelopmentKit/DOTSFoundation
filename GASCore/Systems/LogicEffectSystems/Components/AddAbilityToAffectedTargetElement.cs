namespace GASCore.Systems.LogicEffectSystems.Components
{
    using System;
    using System.Collections.Generic;
    using GASCore.Interfaces;
    using Unity.Collections;
    using Unity.Entities;

    public struct AddAbilityToAffectedTargetElement : IBufferElementData
    {
        public FixedString64Bytes AbilityId;
        public int                Level;
    }
    
    public class AddAbilities : IAbilityActionComponentConverter
    {
        [Serializable]
        public class AbilityInfo
        {
            public string AbilityId;
            public int    Level;
        }

        public List<AbilityInfo> AbilityInfos = new();

        public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
        {
            var bufferElement = ecb.AddBuffer<AddAbilityToAffectedTargetElement>(index, entity);

            foreach (var abilityInfo in this.AbilityInfos)
            {
                bufferElement.Add(new AddAbilityToAffectedTargetElement()
                {
                    AbilityId = abilityInfo.AbilityId,
                    Level     = abilityInfo.Level
                });
            }
        }
    }
}