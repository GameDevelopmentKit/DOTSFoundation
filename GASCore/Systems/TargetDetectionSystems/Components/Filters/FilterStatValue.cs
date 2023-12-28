namespace GASCore.Systems.TargetDetectionSystems.Components.Filters
{
    using System.Collections.Generic;
    using GASCore.Services;
    using Sirenix.OdinInspector;
    using Unity.Collections;
    using Unity.Entities;

    public struct FilterStatValue : IComponentData
    {
        public FixedString64Bytes StatName;
        public float              Value;
        public bool               Percent;
        public bool               Above;
        
        public class Option : FindTargetAuthoring.IOptionConverter
        {
            [ValueDropdown("GetFieldValues")]
            public string StatName;

            public float Value;
            public bool  Percent;
            public bool  Above;

            public List<string> GetFieldValues() => AbilityHelper.GetListStatName();
            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
            {
                ecb.AddComponent(index, entity, new FilterStatValue()
                {
                    StatName = string.IsNullOrEmpty(this.StatName) ? default : new FixedString64Bytes(this.StatName),
                    Value    = this.Value,
                    Percent  = this.Percent,
                    Above    = this.Above,
                });
            }
        }
    }
}