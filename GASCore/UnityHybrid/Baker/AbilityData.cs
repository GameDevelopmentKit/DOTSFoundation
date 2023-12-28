namespace GASCore.UnityHybrid.Baker
{
    using System.Collections.Generic;
    using GASCore.Systems.CasterSystems.Components;
    using Unity.Entities;
    using UnityEngine;

    public class AbilityData : MonoBehaviour
    {
        public List<RequestAddOrUpgradeAbilities.AbilityInfo> AbilityInfos = new();
    }

    public class AbilityDataBaker : Baker<AbilityData>
    {
        public override void Bake(AbilityData authoring)
        {
            var entity        = this.GetEntity(TransformUsageFlags.Dynamic);
            var bufferElement = this.AddBuffer<RequestAddOrUpgradeAbility>(entity);

            foreach (var abilityInfo in authoring.AbilityInfos)
            {
                bufferElement.Add(new RequestAddOrUpgradeAbility(abilityInfo.AbilityId, abilityInfo.Level, abilityInfo.IsAddPrefab));
            }
        }
    }
}