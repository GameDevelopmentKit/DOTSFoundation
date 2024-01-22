namespace GASCore.UnityHybrid.ViewMono
{
    using GASCore.Systems.StatSystems.Components;
    using GASCore.UnityHybrid.HealthBar;
    using Unity.Collections;
    using UnityEngine;

    public class HealthBarView : OnStatChangeExecutor
    {
        [SerializeField] private UIHpBarView uiHpBarView;
        [SerializeField] private bool        enableAtStart = true;

        public override FixedString64Bytes StatName => Systems.StatSystems.Components.StatName.Health;

        private void OnEnable() { this.uiHpBarView.gameObject.SetActive(this.enableAtStart); }

        public override void InitStatView(StatDataElement stat) { this.uiHpBarView.Init(stat.CurrentValue, stat.OriginValue, 0); }

        public override void UpdateStatView(StatDataElement health, float _)
        {
            if (this.uiHpBarView.gameObject.activeSelf == false)
                this.uiHpBarView.gameObject.SetActive(true);
            this.uiHpBarView.UpdateHealBar(health.CurrentValue, 0);
        }
    }
}