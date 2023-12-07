namespace DOTSCore.CommonSystems.Systems
{
    using System;
    using System.Collections.Generic;
    using DOTSCore.CommonSystems.Components;
    using Unity.Entities;
    using UnityEngine;

    [UpdateInGroup(typeof(PresentationSystemGroup), OrderLast = true)]
    public partial class TriggerAnimationSystem : SystemBase
    {
        private Dictionary<AnimationTrigger, int> animationStateToHashMap;
        protected override void OnCreate()
        {
            this.animationStateToHashMap = new Dictionary<AnimationTrigger, int>();
            foreach (AnimationTrigger state in Enum.GetValues(typeof(AnimationTrigger)))
            {
                this.animationStateToHashMap.Add(state, Animator.StringToHash(state.ToString()));
            }
        }
        protected override void OnUpdate()
        {
            this.Entities.WithoutBurst().WithChangeFilter<AnimationTriggerComponent>().ForEach((in AnimationTriggerComponent curAnimState, in AnimatorHybridLink animatorHybridLink) =>
            {
                animatorHybridLink.Value.SetTrigger(this.animationStateToHashMap[curAnimState.Value]);
            }).Run();
        }
    }
}