namespace GASCore.UnityHybrid.ViewMono
{
    using System;
    using System.Collections.Generic;
    using GASCore.Systems.StatSystems.Components;
    using Unity.Collections;
    using UnityEngine;
    using Random = UnityEngine.Random;

    public class DamagePopupView : TextPopupView
    {
        [Serializable]
        private class _Color
        {
            public float damage;
            public Color color;

            public void Deconstruct(out float damage, out Color color)
            {
                damage = this.damage;
                color  = this.color;
            }
        }

        [SerializeField] private List<_Color> colors;

        private Color GetColor(float currentDamage)
        {
            foreach (var (damage, color) in this.colors)
            {
                if (currentDamage >= damage) return color;
            }

            return this.colors[^1].color;
        }

        public override FixedString64Bytes StatName => Systems.StatSystems.Components.StatName.Health;

        protected override void Awake()
        {
            base.Awake();
            this.colors.Sort((a, b) => b.damage.CompareTo(a.damage));
        }

        public override void InitStatView(StatDataElement _)
        {
        }

        public override void UpdateStatView(StatDataElement _, float dmg)
        {
            this.PopupText($"{dmg:N0}", this.GetColor(-dmg), new(Random.Range(-1f, 1f), Random.Range(1f, 2f), Random.Range(-1f, 1f)));
        }
    }
}