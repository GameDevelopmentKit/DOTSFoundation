namespace GASCore.Systems.VisualEffectSystems.Components
{
    using DOTSCore.CommonSystems.Components;
    using GASCore.Interfaces;
    using Unity.Entities;

    public class AnimationTriggerAuthoring : IAbilityActionComponentConverter
    {
        public AnimationTrigger Trigger;
        public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
        {
            ecb.AddComponent(index,entity, new AnimationTriggerComponent(){Value = this.Trigger});
        }
    }
}