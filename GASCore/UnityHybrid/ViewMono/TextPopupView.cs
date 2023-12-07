namespace GASCore.UnityHybrid.ViewMono
{
    using DG.Tweening;
    using GameFoundation.Scripts.Utilities.Extension;
    using GameFoundation.Scripts.Utilities.ObjectPool;
    using GASCore.Systems.StatSystems.Components;
    using TMPro;
    using UnityEngine;
    using Zenject;

    public abstract class TextPopupView : OnStatChangeExecutor
    {
        [SerializeField] private TextMeshPro textPrefab;
        [SerializeField] private float       fadeInTime           = 0.25f;
        [SerializeField] private float       textScale            = 1.1f;
        [SerializeField] private Vector3     randomPositionOffset = new(1f, 0.5f, 1f);

        [Inject] private ObjectPoolManager textPool;

        protected virtual void Awake()
        {
            this.GetCurrentContainer().Inject(this);
            this.textPool.CreatePool(this.textPrefab, 10,null);
        }

        public override void InitStatView(StatDataElement stat)
        {
        }

        public override void UpdateStatView(StatDataElement stat, float value)
        {
            // TODO: show a small icon for the stat
            var positionOffset = new Vector3(
                Random.Range(-this.randomPositionOffset.x, this.randomPositionOffset.x),
                Random.Range(-this.randomPositionOffset.y, this.randomPositionOffset.y) + 1.5f,
                Random.Range(-this.randomPositionOffset.z, this.randomPositionOffset.z)
            );

            var instance = this.textPool.Spawn(this.textPrefab, this.transform.position + positionOffset, Quaternion.Euler(45f, 0f, 0f));
            instance.text  = $"{value:N0}";
            instance.color = this.GetColor(stat, value);

            var seq = DOTween.Sequence();
            seq.Append(instance.transform.DOLocalMoveY(instance.transform.position.y + 1f, this.fadeInTime));
            seq.Join(instance.transform.DOScale(this.textScale, this.fadeInTime));
            seq.OnComplete(instance.Recycle);
        }

        protected abstract Color  GetColor(StatDataElement _, float value);
        protected abstract Sprite GetSprite(StatDataElement stat, float value);
    }
}