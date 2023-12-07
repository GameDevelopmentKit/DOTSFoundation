namespace GASCore.UnityHybrid.ViewMono
{
    using GASCore.Systems.StatSystems.Components;
    using Unity.Collections;
    using UnityEngine;

    public class CoinPopupView : TextPopupView
    {
        [SerializeField] private Color color = Color.yellow;

        public override FixedString64Bytes StatName => Systems.StatSystems.Components.StatName.Coin;

        protected override Color  GetColor(StatDataElement _, float value)
        {
            return this.color;
        }

        protected override Sprite GetSprite(StatDataElement stat, float value)
        {
            return null;
        }
    }
}