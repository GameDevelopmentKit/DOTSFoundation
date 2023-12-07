namespace GASCore.UnityHybrid.Baker
{
    using System.Collections.Generic;
    using GASCore.Systems.StatSystems.Components;
    using Unity.Entities;
    using UnityEngine;

    public class StatData : MonoBehaviour
    {
        public List<StatDataAuthoring.StatElement> Stats;
    }

    public class StatDataBaker : Baker<StatData>
    {
        public override void Bake(StatData authoring)
        {
            var statBuffer = this.AddBuffer<StatDataElement>(this.GetEntity(TransformUsageFlags.Dynamic));

            foreach (var stat in authoring.Stats)
            {
                statBuffer.Add(new StatDataElement()
                {
                    StatName    = stat.StatName,
                    OriginValue = stat.BaseValue,
                    BaseValue   = stat.BaseValue,
                    AddedValue  = 0
                });
            }
        }
    }
}