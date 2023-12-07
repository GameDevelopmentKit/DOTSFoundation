namespace GASCore.Systems.VisualEffectSystems.Components
{
    using GASCore.Interfaces;
    using Unity.Entities;
    public struct MoveToRandomTargetInRange : IComponentData
    {
        public float Range;
        public class _: IAbilityActionComponentConverter
        {
            public float Range;
            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
            {
                ecb.AddComponent(index,entity,new MoveToRandomTargetInRange(){Range = this.Range});
            }
        }
    }
}