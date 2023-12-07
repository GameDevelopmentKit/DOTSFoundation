namespace GASCore.Systems.LogicEffectSystems.Components
{
    using GASCore.Interfaces;
    using Unity.Entities;

    public struct TeamOwnerId : IComponentData
    {
        public int Value;

        public class _ : IAbilityActionComponentConverter
        {
            public TeamType Value;

            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity) { ecb.AddComponent(index, entity, new TeamOwnerId() { Value = (int)this.Value }); }
        }
    }

    public enum TeamType
    {
        None      = 0,
        Character = 1,
        Monster   = 2,
    }
}