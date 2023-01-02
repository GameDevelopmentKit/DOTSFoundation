namespace GASCore.Systems.AbilityMainFlow.Components
{
    using GASCore.Interfaces;
    using Unity.Entities;

    public struct AutoActiveOnStartTag : IComponentData
    {
        public class _ : IAbilityActivateConditionConverter
        {
            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity) { ecb.AddComponent<AutoActiveOnStartTag>(index, entity); }
        }
    }
}