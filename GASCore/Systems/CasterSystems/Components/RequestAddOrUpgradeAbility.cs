namespace GASCore.Systems.CasterSystems.Components
{
    using System;
    using System.Collections.Generic;
    using GASCore.Interfaces;
    using GASCore.Systems.LogicEffectSystems.Components;
    using Unity.Collections;
    using Unity.Entities;

    public struct RequestAddOrUpgradeAbility : IBufferElementData
    {
        public   FixedString64Bytes AbilityId { get;}
        public   int                Level     { get; }
        public   bool               IsAddPrefab  { get; }
        internal FixedString64Bytes AbilityLevelKey;
        public RequestAddOrUpgradeAbility(FixedString64Bytes abilityId, int level, bool isAddPrefab = false)
        {
            this.AbilityId       = abilityId;
            this.Level           = level;
            this.IsAddPrefab     = isAddPrefab;
            abilityId.Append('_');
            abilityId.Append(level);
            this.AbilityLevelKey = abilityId;
        }
    }
    
    public class RequestAddOrUpgradeAbilities : IAbilityActionComponentConverter
    {
        [Serializable]
        public class AbilityInfo
        {
            public string AbilityId;
            public int    Level;
            public bool   IsAddPrefab = true;
        }

        public List<AbilityInfo> AbilityInfos = new();

        public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
        {
            var bufferElement = ecb.AddBuffer<RequestAddOrUpgradeAbility>(index, entity);

            foreach (var abilityInfo in this.AbilityInfos)
            {
                bufferElement.Add(new RequestAddOrUpgradeAbility(abilityInfo.AbilityId, abilityInfo.Level, abilityInfo.IsAddPrefab));
            }
        }
    }
}