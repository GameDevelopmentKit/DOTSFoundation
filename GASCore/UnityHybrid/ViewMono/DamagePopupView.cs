namespace GASCore.UnityHybrid.ViewMono
{
    using System;
    using System.Collections.Generic;
    using DG.Tweening;
    using GameFoundation.Scripts.Utilities.Extension;
    using GameFoundation.Scripts.Utilities.ObjectPool;
    using GASCore.Systems.StatSystems.Components;
    using TMPro;
    using Unity.Collections;
    using UnityEngine;
    using Zenject;
    using Random = UnityEngine.Random;

    public class DamagePopupView : OnStatChangeExecutor
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

        [Inject] private ObjectPoolManager textPool;

        [SerializeField] private TextMeshPro textPrefab;
        [SerializeField] private float       fadeInTime  = 1f;
        [SerializeField] private float       fadeOutTime = 0.5f;
        [SerializeField] private float       textScale   = 1.5f;

        [SerializeField] private List<_Color> colors = new()
        {
            new _Color { damage = 0, color    = Color.white },
            new _Color { damage = 400, color  = Color.yellow },
            new _Color { damage = 1000, color = Color.red },
        };

        private void Awake()
        {
            this.GetCurrentContainer().Inject(this);
            this.colors.Sort((a, b) => b.damage.CompareTo(a.damage));
        }

        private Color GetColor(float dmg)
        {
            foreach (var (damage, color) in this.colors)
            {
                if (dmg >= damage) return color;
            }

            return Color.white;
        }

        private void PopupDmg(float dmg)
        {
            var text    = this.textPool.Spawn(this.textPrefab);
            var color   = this.GetColor(dmg);
            var thisPos = this.transform.position;
            var textPos = new Vector3(thisPos.x + Random.Range(-1f, 1f), thisPos.y + Random.Range(1f, 2f), thisPos.z + Random.Range(-1f, 1f));
            text.transform.position = textPos;
            text.transform.rotation = Quaternion.Euler(45f, 0f, 0f);
            text.text               = $"-{dmg:N0}";
            color.a                 = 0f;
            text.color              = color;

            var seq = DOTween.Sequence();
            seq.Append(text.transform.DOLocalMoveY(textPos.y + 1, this.fadeInTime));
            seq.Join(text.transform.DOScale(this.textScale, this.fadeInTime));
            seq.Join(text.DOFade(1f, this.fadeInTime));
            seq.Append(text.DOFade(0f, this.fadeOutTime));
            seq.OnComplete(text.Recycle);
        }

        public override FixedString64Bytes StatName => Systems.StatSystems.Components.StatName.Health;

        public override void Execute(StatDataElement changedStat, float changedValue)
        {
            this.PopupDmg(-changedValue);
        }
    }
}