namespace GASCore.Systems.VisualEffectSystems.Systems
{
    using DOTSCore.CommonSystems.Components;
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using GASCore.Systems.VisualEffectSystems.Components;
    using Unity.Burst;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityVisualEffectGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct TriggerAnimationSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var job = new ChangeAnimationStateJob()
            {
                AnimationStateLookup = SystemAPI.GetComponentLookup<AnimationTriggerComponent>()
            }.Schedule(state.Dependency);
            job.Complete();
        }
    }

    [BurstCompile]
    [WithNone(typeof(EndTimeComponent))]
    public partial struct ChangeAnimationStateJob : IJobEntity
    {
        public ComponentLookup<AnimationTriggerComponent> AnimationStateLookup;
        void Execute(in PlayAnimation playAnimation, AffectedTargetComponent affectedTarget)
        {
            if (this.AnimationStateLookup.TryGetComponent(affectedTarget.Value, out var animationState))
            {
                animationState.Value                            = playAnimation.Value;
                this.AnimationStateLookup[affectedTarget.Value] = animationState;
            }
        }
    }
}