namespace GASCore.Systems.StatSystems.Components
{
    using GASCore.Interfaces;
    using Unity.Entities;

    public interface IDurationPolicy : IComponentData { }

    public struct InstantEffect : IDurationPolicy
    {
        public class _ : IAbilityActionComponentConverter
        {
            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity) { ecb.AddComponent(index, entity, new InstantEffect()); }
        }
    }

    public struct DurationEffect : IDurationPolicy
    {
        public float Value;

        public class _ : IAbilityActionComponentConverter
        {
            public float Value;
            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
            {
                ecb.AddComponent(index, entity, new DurationEffect()
                {
                    Value = this.Value
                });
            }
        }
    }

    public struct InfiniteEffect : IDurationPolicy
    {
        public class _ : IAbilityActionComponentConverter
        {
            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity) { ecb.AddComponent(index, entity, new InfiniteEffect()); }
        }
    }

    /// <summary>
    /// Go along with DurationEffect or InfiniteEffect
    /// </summary>
    public struct PeriodEffect : IDurationPolicy
    {
        public float Value;

        public class _ : IAbilityActionComponentConverter
        {
            public float PeriodInSecond;
            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
            {
                ecb.AddComponent(index, entity, new PeriodEffect()
                {
                    Value = this.PeriodInSecond
                });
            }
        }
    }
    
    //todo seem this component is useless
    public struct PeriodEffectInstanceTag: IComponentData{}
}