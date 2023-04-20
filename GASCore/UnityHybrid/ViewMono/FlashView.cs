namespace GASCore.UnityHybrid.StatMonoAction
{
    using System;
    using Cysharp.Threading.Tasks;
    using GASCore.Systems.StatSystems.Components;
    using GASCore.UnityHybrid.ViewMono;
    using Unity.Collections;
    using UnityEngine;

    public class FlashView : OnStatChangeExecutor
    {
        [SerializeField] private float duration   = 0.25f;
        [SerializeField] private float flashValue = 0.6f;

        private Material material;

        private static readonly int Flash = Shader.PropertyToID("_flash");

        private void Awake()
        {
            this.material = this.GetComponentInChildren<SkinnedMeshRenderer>().materials[0];
        }

        private async UniTask FlashHit()
        {
            this.material.SetFloat(Flash, this.flashValue);
            await UniTask.Delay(TimeSpan.FromSeconds(this.duration));
            this.material.SetFloat(Flash, 0);
        }

        public override FixedString64Bytes StatName => Systems.StatSystems.Components.StatName.Health;

        public override void InitStatView(StatDataElement _)
        {
        }

        public override void UpdateStatView(StatDataElement _, float __)
        {
            this.FlashHit().Forget();
        }
    }
}