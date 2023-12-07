namespace GASCore.UnityHybrid.ViewMono
{
    using System.Collections.Generic;
    using GASCore.Systems.StatSystems.Components;
    using Unity.Collections;
    using UnityEngine;

    public class DamagePopupView : TextPopupView
    {
        [SerializeField] private List<Element<Color>> colors = new();

        public override FixedString64Bytes StatName => Systems.StatSystems.Components.StatName.Health;

        protected override void Awake()
        {
            base.Awake();
            this.colors.Init();
        }

        protected override Color GetColor(StatDataElement _, float value)
        {
            return this.colors.Get(value);
        }

        protected override Sprite GetSprite(StatDataElement stat, float value)
        {
            return null;
        }
    }
}