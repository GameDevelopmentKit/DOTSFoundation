namespace GASCore.Systems.VisualEffectSystems.Components
{
    using DOTSCore.CommonSystems.Components;
    using GASCore.Interfaces;
    using Unity.Entities;

    public struct PlayAnimation : IComponentData
    {
        public AnimationTrigger Value;
        
        public class _ : IAbilityActionComponentConverter
        {
            public AnimationTrigger Value;
            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
            {
                ecb.AddComponent(index, entity, new PlayAnimation(){ Value = this.Value});
            }
        }
    }
}