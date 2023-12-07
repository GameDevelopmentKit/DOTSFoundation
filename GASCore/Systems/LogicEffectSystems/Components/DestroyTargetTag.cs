namespace GASCore.Systems.LogicEffectSystems.Components
{
    using GASCore.Interfaces;
    using Unity.Entities;

    public struct DestroyTargetTag : IComponentData
    {
        public class Tag : IAbilityActionComponentConverter
        {
            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
            {
                ecb.AddComponent<DestroyTargetTag>(index, entity);
            }
        }
    }
}