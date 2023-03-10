namespace GASCore.Systems.TargetDetectionSystems.Components
{
    using System.Collections.Generic;
    using GASCore.Interfaces;
    using Unity.Collections;
    using Unity.Entities;

    public struct TargetWithStatElement : IBufferElementData
    {
        public                          FixedString64Bytes Value;
        public static implicit operator FixedString64Bytes(TargetWithStatElement statName) => statName.Value;
        public static implicit operator TargetWithStatElement(FixedString64Bytes statName) => new() { Value = statName };
        public static implicit operator TargetWithStatElement(string statName)             => new() { Value = statName };
    }

    public class FindTargetWithStats : ITriggerConditionActionConverter
    {
        public List<string> StatNames;

        public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
        {
            var statNames = ecb.AddBuffer<TargetWithStatElement>(index, entity);
            foreach (var statName in this.StatNames)
            {
                statNames.Add(statName);
            }
        }
    }
}