namespace GASCore.Systems.VisualEffectSystems.Systems
{
    using DOTSCore.CommonSystems.Components;
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.VisualEffectSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using UnityEngine;

    [UpdateInGroup(typeof(AbilityVisualEffectGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct ChangeAnimationStateSystem : ISystem
    {
        ComponentLookup<AnimationStateComponent> animationStateLookup;
        [BurstCompile]
        public void OnCreate(ref SystemState state) { animationStateLookup = state.GetComponentLookup<AnimationStateComponent>(true); }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            this.animationStateLookup.Update(ref state);
            var ecbSingleton = SystemAPI.GetSingleton<AbilityPresentEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            new ChangeAnimationStateJob()
            {
                Ecb = ecb,
                AnimationStateLookup = animationStateLookup
            }.ScheduleParallel();
        }
    }

    [BurstCompile]
    [WithChangeFilter(typeof(PlayAnimation))]
    public partial struct ChangeAnimationStateJob : IJobEntity
    {
        public            EntityCommandBuffer.ParallelWriter       Ecb;
        [ReadOnly] public ComponentLookup<AnimationStateComponent> AnimationStateLookup;
        void Execute([EntityIndexInQuery] int entityInQueryIndex, in PlayAnimation playAnimation, AffectedTargetComponent affectedTarget)
        {
            if (this.AnimationStateLookup.TryGetComponent(affectedTarget.Value, out var animationState))
            {
                animationState.ChangeState(playAnimation.Value);
                this.Ecb.SetComponent(entityInQueryIndex, affectedTarget.Value, animationState);
            }
        }
    }
}