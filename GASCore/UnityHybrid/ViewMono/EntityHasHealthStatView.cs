namespace GASCore.UnityHybrid.ViewMono
{
    using GASCore.Systems.StatSystems.Components;
    using GASCore.UnityHybrid.HealthBar;
    using UnityEngine;

    public class EntityHasHealthStatView : BaseEntityStatView
    {
        [SerializeField] private UIHpBarView uiHpBarView;

        protected override void InitStatView(StatDataElement data)
        {
            if (data.StatName.Value == StatName.Health)
            {
                this.uiHpBarView.Init(data.CurrentValue, data.BaseValue, 0);
            }
        }

        protected override void ChangeStatView(StatDataElement changedStat, float changedValue)
        {
            if (changedStat.StatName.Value == StatName.Health)
            {
                this.uiHpBarView.UpdateHealBar(changedStat.CurrentValue, 0);
            }
        }
    }
}