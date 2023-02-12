namespace GASCore.Systems.VisualEffectSystems.Components
{
    using GASCore.Interfaces;
    using Unity.Entities;

    public struct MoveToAffectedTarget : IComponentData
    {

        public class _ : IAbilityActionComponentConverter
        {
            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity) { ecb.AddComponent<MoveToAffectedTarget>(index, entity); }
        }
    }
}