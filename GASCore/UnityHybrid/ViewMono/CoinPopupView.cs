namespace GASCore.UnityHybrid.ViewMono
{
    using GASCore.Systems.StatSystems.Components;
    using Unity.Collections;
    using UnityEngine;

    public class CoinPopupView : TextPopupView
    {
        [SerializeField] private Color textColor = Color.yellow;

        public override FixedString64Bytes StatName => Systems.StatSystems.Components.StatName.Coin;

        public override void InitStatView(StatDataElement stat)
        {
        }

        public override void UpdateStatView(StatDataElement _, float coin)
        {
            this.PopupText($"{coin:N0}", this.textColor, new(0f, 1f, 0f));
        }
    }
}