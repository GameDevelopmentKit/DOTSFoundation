namespace GASCore.Systems.VisualEffectSystems.Components
{
    using GASCore.Interfaces;
    using Unity.Entities;

    public struct MoveInAffectedTargetDir : IComponentData
    {
        public bool IsReverse;
        public class _ : IAbilityActionComponentConverter
        {
            public bool IsReverse;
            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
            {
                ecb.AddComponent(index,entity, new MoveInAffectedTargetDir(){IsReverse = this.IsReverse});
            }
        }
    }
}