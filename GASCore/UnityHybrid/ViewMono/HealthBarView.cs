namespace GASCore.UnityHybrid.ViewMono
{
    using GASCore.Systems.StatSystems.Components;
    using GASCore.UnityHybrid.HealthBar;
    using Unity.Collections;
    using UnityEngine;

    public class HealthBarView : OnStatChangeExecutor
    {
        [SerializeField] private UIHpBarView uiHpBarView;

        public override FixedString64Bytes StatName => Systems.StatSystems.Components.StatName.Health;

        public override void InitStatView(StatDataElement stat)
        {
            this.uiHpBarView.Init(stat.CurrentValue, stat.BaseValue, 0);
        }

        public override void UpdateStatView(StatDataElement health, float _)
        {
            this.uiHpBarView.UpdateHealBar(health.CurrentValue, 0);
        }
    }
}