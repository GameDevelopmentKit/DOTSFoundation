namespace GASCore.UnityHybrid.HealthBar
{
    using System;
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Utilities.Extension;
    using GameFoundation.Scripts.Utilities.ObjectPool;
    using TMPro;
    using UnityEngine;
    using Zenject;
    using Random = UnityEngine.Random;

    public class DamagePopupView : MonoBehaviour
    {
        [Serializable]
        private class _Color
        {
            public float Damage;
            public Color Color;

            public void Deconstruct(out float damage, out Color color)
            {
                damage = this.Damage;
                color  = this.Color;
            }
        }

        [Inject] private ObjectPoolManager dmgPopupPool;

        [SerializeField] private TextMeshPro  dmgPopupText;
        [SerializeField] private float        dmgPopupTime = 1f;
        [SerializeField] private List<_Color> dmgPopupColor;

        public void Awake()
        {
            this.GetCurrentContainer().Inject(this);
            this.dmgPopupColor.Sort((a, b) => a.Damage.CompareTo(b.Damage));
        }

        private Color GetColor(float dmg)
        {
            foreach (var (damage, color) in this.dmgPopupColor)
            {
                if (dmg >= damage) return color;
            }

            return Color.white;
        }

        public async UniTask PopupDmg(float dmg)
        {
            var text    = this.dmgPopupPool.Spawn(this.dmgPopupText);
            var thisPos = this.transform.position;
            text.transform.position = new Vector3(thisPos.x + Random.Range(-1f, 1f), thisPos.y + Random.Range(1f, 2f), thisPos.z + Random.Range(-1f, 1f));
            text.transform.rotation = Quaternion.Euler(45f, 0f, 0f);
            text.text               = $"-{dmg}";
            text.color              = this.GetColor(dmg);
            await UniTask.Delay(TimeSpan.FromSeconds(this.dmgPopupTime));
            text.Recycle();
        }
    }
}