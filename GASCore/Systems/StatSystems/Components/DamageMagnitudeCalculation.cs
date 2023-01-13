namespace GASCore.Systems.StatSystems.Components
{
    using GASCore.Interfaces;
    using Unity.Entities;

    public class DamageStatModifierEntityAuthoring : StatModifierEntityAuthoring
    {
        public override void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
        {
            base.Convert(ecb, index, entity);
            ecb.AddComponent<DealDamageTag>(index, entity);
        }
    }

    public struct DealDamageTag : IComponentData
    {
    }

    public struct DamageStatModifierData : IComponentData
    {
        public class _ : IStatModifierComponentConverter
        {
            [Sirenix.OdinInspector.ReadOnly] public string Stat = StatName.Damage.Value;

            public ModifierOperatorType ModifierOperator;
            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
            {
                ecb.AddComponent(index, entity, new StatModifierData()
                {
                    TargetStat       = this.Stat,
                    ModifierOperator = this.ModifierOperator
                });
            }
        }
    }
}