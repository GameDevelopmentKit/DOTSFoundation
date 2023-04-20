namespace GASCore.Systems.StatSystems.Components
{
    using GASCore.Interfaces;
    using Unity.Collections;
    using Unity.Entities;

    public struct UpdateKillCountStatNameComponent : IComponentData
    {
        public                          FixedString64Bytes Value;
        public static implicit operator FixedString64Bytes(UpdateKillCountStatNameComponent statName) => statName.Value;
        public static implicit operator UpdateKillCountStatNameComponent(FixedString64Bytes statName) => new() { Value = statName };
    }

    public struct UpdateKillCountStat : IComponentData
    {
        public class _ : IAbilityActionComponentConverter
        {
            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
            {
                ecb.AddComponent<UpdateKillCountStat>(index, entity);
                ecb.AddBuffer<ModifierAggregatorData>(index, entity);
            }
        }
    }
}