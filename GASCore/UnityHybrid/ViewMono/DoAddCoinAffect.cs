namespace GASCore.UnityHybrid.ViewMono
{
    using DG.Tweening;
    using GameFoundation.Scripts.Utilities.Extension;
    using GameFoundation.Scripts.Utilities.ObjectPool;
    using GASCore.Systems.StatSystems.Components;
    using TMPro;
    using Unity.Collections;
    using UnityEngine;
    using Zenject;

    public class DoAddCoinAffect : OnStatChangeExecutor
    {
        [SerializeField] private TextMeshPro       textPrefab;
        [SerializeField] private float             fadeInTime           = 0.25f;
        [SerializeField] private float             fadeOutTime          = 0.5f;
        [SerializeField] private float             textScale            = 1.1f;
        [SerializeField] private Vector3           randomPositionOffset = new(1f, 0.5f, 1f);
        [Inject]         private ObjectPoolManager textPool;
        public                   int               value;
        public                   bool              isActive;

        protected virtual void Awake()
        {
            this.GetCurrentContainer().Inject(this);
        }
        public override FixedString64Bytes StatName => Systems.StatSystems.Components.StatName.Health;
        public override void InitStatView(StatDataElement stat)
        {
            
        }
        public override void UpdateStatView(StatDataElement stat, float changedValue)
        {
            if (stat.BaseValue <= 0&&this.isActive)
            {
                this.DoEffect();
            }
        }
        
        public void DoEffect()
        {
            var color         = Color.yellow;
            var originalAlpha = color.a;
            color.a = 0f;

            var positionOffset = new Vector3(
                Random.Range(-this.randomPositionOffset.x, this.randomPositionOffset.x),
                Random.Range(-this.randomPositionOffset.y, this.randomPositionOffset.y) + 1.5f,
                Random.Range(-this.randomPositionOffset.z, this.randomPositionOffset.z)
            );

            var instance = this.textPool.Spawn(this.textPrefab, this.transform.position + positionOffset, Quaternion.Euler(45f, 0f, 0f));
            instance.text  = $"{value:N0}";
            instance.color = color;

            var seq = DOTween.Sequence();
            seq.Append(instance.transform.DOLocalMoveY(instance.transform.position.y + 1f, this.fadeInTime));
            seq.Join(instance.transform.DOScale(this.textScale, this.fadeInTime));
            seq.Join(instance.DOFade(originalAlpha, this.fadeInTime));
            seq.Append(instance.DOFade(0f, this.fadeOutTime));
            seq.OnComplete(instance.Recycle);
        }
    }
}