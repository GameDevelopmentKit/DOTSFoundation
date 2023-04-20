namespace GASCore.UnityHybrid.ViewMono
{
    using DG.Tweening;
    using GameFoundation.Scripts.Utilities.Extension;
    using GameFoundation.Scripts.Utilities.ObjectPool;
    using TMPro;
    using UnityEngine;
    using Zenject;

    public abstract class TextPopupView : OnStatChangeExecutor
    {
        [Inject]         private ObjectPoolManager textPool;
        [SerializeField] private TextMeshPro       textPrefab;
        [SerializeField] private float             fadeInTime  = 1f;
        [SerializeField] private float             fadeOutTime = 0.5f;
        [SerializeField] private float             textScale   = 1.5f;

        protected virtual void Awake()
        {
            this.GetCurrentContainer().Inject(this);
        }

        protected void PopupText(string text, Color color, Vector3 positionOffset)
        {
            var originalAlpha = color.a;
            color.a = 0f;

            var instance = this.textPool.Spawn(this.textPrefab, this.transform.position + positionOffset, Quaternion.Euler(45f, 0f, 0f));
            instance.text  = text;
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