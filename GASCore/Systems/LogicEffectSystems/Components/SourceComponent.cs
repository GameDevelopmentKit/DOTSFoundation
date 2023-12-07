namespace GASCore.Systems.LogicEffectSystems.Components
{
    using GASCore.Interfaces;
    using Unity.Entities;

    public struct SourceComponent : IComponentData
    {
        public                          Entity Value;
        public static implicit operator Entity(SourceComponent source) => source.Value;
        public static implicit operator SourceComponent(Entity source) => new() { Value = source };
    }

    public struct SourceTypeComponent : IComponentData
    {
        public SourceType Value;

        public class _ : IAbilityActionComponentConverter, IStatModifierComponentConverter
        {
            public SourceType Value;

            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
            {
                ecb.AddComponent(index, entity, new SourceTypeComponent() { Value = this.Value });
            }
        }
    }

    public enum SourceType
    {
        Caster,
        AffectedTarget,
        Self,
    }
}