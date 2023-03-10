namespace GASCore.UnityHybrid.StatMonoAction
{
    using System;
    using Cysharp.Threading.Tasks;
    using GASCore.Systems.StatSystems.Components;
    using GASCore.UnityHybrid.ViewMono;
    using Unity.Collections;
    using UnityEngine;

    public class FlashExecutorMono : OnStatChangeExecutor
    {
        private Material material;
        private Color    currentColor;

        private void Start()
        {
            this.material     = this.GetComponentInChildren<SkinnedMeshRenderer>().materials[0];
            this.currentColor = this.material.color;
        }

        private async UniTask FlashHit()
        {
            this.material.color = Color.black;
            await UniTask.Delay(TimeSpan.FromSeconds(0.5));
            this.material.color = this.currentColor;
        }

        public override FixedString64Bytes StatName => Systems.StatSystems.Components.StatName.Health;

        public override async void Execute(StatDataElement changedStat, float changedValue)
        {
            await this.FlashHit();
        }
    }
}