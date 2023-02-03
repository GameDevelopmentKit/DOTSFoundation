namespace GASCore.Systems.VisualEffectSystems.Components
{
    using GASCore.Interfaces;
    using Unity.Entities;

    public struct MoveCasterToAffectedTarget : IComponentData
    {
        public bool IsChase;

        public class _ : IAbilityActionComponentConverter
        {
            public bool IsChase;
            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity) { ecb.AddComponent(index, entity, new MoveCasterToAffectedTarget() { IsChase = this.IsChase }); }
        }
    }
}