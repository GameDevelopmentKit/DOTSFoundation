namespace GASCore.Systems.TimelineSystems.Components
{
    using System.Collections.Generic;
    using GASCore.Interfaces;
    using GASCore.Services;
    using GASCore.Systems.StatSystems.Components;
    using Sirenix.OdinInspector;
    using Unity.Collections;
    using Unity.Entities;

    //Listener on stat changed
    public struct TriggerOnStatChanged : IComponentData
    {
        public bool               AnyStat;
        public FixedString64Bytes StatName;
        public float              Value;
        public bool               Percent;
        public bool               Above;

        public class _ : ITriggerConditionActionConverter, IAbilityActivateConditionConverter
        {
            public bool AnyStat;

            [ValueDropdown("GetFieldValues", AppendNextDrawer = true), HideIf("AnyStat")]
            public string StatName;

            public float Value;
            public bool  Percent;
            public bool  Above;

            public List<string> GetFieldValues() => AbilityHelper.GetListStatName();

            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
            {
                ecb.AddComponent(index, entity, new TriggerOnStatChanged()
                {
                    AnyStat  = this.AnyStat,
                    StatName = string.IsNullOrEmpty(this.StatName) ? default : new FixedString64Bytes(this.StatName),
                    Value    = this.Value,
                    Percent  = this.Percent,
                    Above    = this.Above,
                });
            }
        }
    }

    // signal on stat change
    public struct OnStatChangeTag : IComponentData, IEnableableComponent { }

    public struct StatChangeElement : IBufferElementData
    {
        public StatDataElement Value;
    }
}