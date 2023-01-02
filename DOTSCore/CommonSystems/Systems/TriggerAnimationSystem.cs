namespace DOTSCore.CommonSystems.Systems
{
    using System;
    using System.Collections.Generic;
    using DOTSCore.CommonSystems.Components;
    using Unity.Entities;
    using UnityEngine;
    using AnimationState = DOTSCore.CommonSystems.Components.AnimationState;

    [UpdateInGroup(typeof(PresentationSystemGroup), OrderLast = true)]
    public partial class TriggerAnimationSystem : SystemBase
    {
        private Dictionary<AnimationState, int> animationStateToHashMap;
        protected override void OnCreate()
        {
            this.animationStateToHashMap = new Dictionary<AnimationState, int>();
            foreach (AnimationState state in Enum.GetValues(typeof(AnimationState)))
            {
                this.animationStateToHashMap.Add(state, Animator.StringToHash(state.ToString()));
            }
        }
        protected override void OnUpdate()
        {
            this.Entities.WithoutBurst().ForEach((ref AnimationStateComponent curAnimState, in GameObjectHybridLink hybridLink) =>
            {
                if (!curAnimState.IsChangeState) return;
                hybridLink.Animator.SetTrigger(this.animationStateToHashMap[curAnimState.Value]);
                curAnimState.IsChangeState = false;
            }).Run();
        }
    }
}