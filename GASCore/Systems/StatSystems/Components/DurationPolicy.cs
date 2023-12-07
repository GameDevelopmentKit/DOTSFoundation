namespace GASCore.Systems.StatSystems.Components
{
    using GASCore.Interfaces;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
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
        public class _ : IAbilityActionComponentConverter
        {
            public float Value;
            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
            {
                ecb.AddComponent(index, entity, new DurationEffect());
                ecb.AddComponent(index, entity, new Duration() { Value = this.Value });
            }
        }
    }

    public struct InfiniteEffect : IDurationPolicy
    {
        public class _ : IAbilityActionComponentConverter
        {
            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
            {
                ecb.AddComponent(index, entity, new InfiniteEffect());
                ecb.AddComponent<IgnoreCleanupTag>(index, entity);
            }
        }
    }

    /// <summary>
    /// Go along with DurationEffect or InfiniteEffect
    /// </summary>
    public struct PeriodEffect : IDurationPolicy
    {
        public class _ : IAbilityActionComponentConverter
        {
            public float PeriodInSecond;
            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
            {
                ecb.AddComponent(index, entity, new PeriodEffect());
                
                ecb.AddComponent(index, entity, new EndTimeComponent(){AmountTime = this.PeriodInSecond});
                ecb.SetComponentEnabled<EndTimeComponent>(index, entity, false);
            }
        }
    }
}