namespace GASCore.Systems.TargetDetectionSystems.Components.Filters
{
    using System.Collections.Generic;
    using GASCore.Services;
    using Sirenix.OdinInspector;
    using Unity.Collections;
    using Unity.Entities;

    public struct FilterKillCounter : IComponentData
    {
        public class Option : IFilterTargetConverter
        {
            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
            {
                ecb.AddComponent<FilterKillCounter>(index, entity);
                ecb.AddBuffer<FilterIncludeStatName>(index, entity);
            }
        }
    }

    public struct FilterIncludeStatName : IBufferElementData
    {
        public                          FixedString64Bytes Value;
        public static implicit operator FixedString64Bytes(FilterIncludeStatName statName) => statName.Value;
        public static implicit operator FilterIncludeStatName(string statName)             => new() { Value = statName };
        public static implicit operator FilterIncludeStatName(FixedString64Bytes statName) => new() { Value = statName };

        public class Option : IFilterTargetConverter
        {
            [ValueDropdown("GetFieldValues", AppendNextDrawer = true)]
            public List<string> StatNames;
            public List<string> GetFieldValues() => AbilityHelper.GetListStatName();
            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
            {
                var statsNamesToInclude = ecb.AddBuffer<FilterIncludeStatName>(index, entity);
                foreach (var statName in this.StatNames)
                {
                    statsNamesToInclude.Add((FilterIncludeStatName)statName);
                }
            }
        }
    }
}