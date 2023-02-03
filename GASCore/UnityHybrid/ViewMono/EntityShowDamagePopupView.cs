namespace GASCore.UnityHybrid.ViewMono
{
    using GASCore.Systems.StatSystems.Components;
    using GASCore.UnityHybrid.HealthBar;
    using UnityEngine;

    public class EntityShowDamagePopupView : BaseEntityStatView
    {
        [SerializeField] private DamagePopupView damagePopupView;

        protected override void ChangeStatView(float changeValue, StatDataElement data)
        {
            if (data.StatName == StatName.Health && changeValue < 0)
            {
                this.damagePopupView.PopupDmg(-changeValue);
            }
        }
    }
}