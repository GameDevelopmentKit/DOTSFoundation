namespace GASCore.Systems.VisualEffectSystems.Components
{
    using DOTSCore.CommonSystems.Components;
    using GASCore.Interfaces;
    using Unity.Entities;

    public class IgnoreSysnTransformAuthoring : IAbilityActionComponentConverter
    {
        public bool IsAddForAffectTarget = false;
        public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
        {
            ecb.AddComponent(index,entity,new IgnoreSysnTransformComponent(){IsAddForAffectTarget = this.IsAddForAffectTarget});
        }
    }
}