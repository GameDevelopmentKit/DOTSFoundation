namespace GASCore.UnityHybrid.Baker
{
    using System.Collections.Generic;
    using GASCore.Systems.LogicEffectSystems.Components;
    using Unity.Entities;
    using UnityEngine;

    public class AbilityData : MonoBehaviour
    {
        public List<AddAbilities.AbilityInfo> AbilityInfos = new();
    }
    
    public class AbilityDataBaker : Baker<AbilityData>
    {
        public override void Bake(AbilityData authoring)
        {
            var bufferElement = this.AddBuffer<AddAbilityElement>();

            foreach (var abilityInfo in authoring.AbilityInfos)
            {
                bufferElement.Add(new AddAbilityElement()
                {
                    AbilityId = abilityInfo.AbilityId,
                    Level     = abilityInfo.Level
                });
            }
        }
    }
}