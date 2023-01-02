namespace GASCore.Systems.AbilityMainFlow.Components
{
    using GASCore.Interfaces;
    using Unity.Entities;

    public struct AutoActiveAfterCooldownTag : IComponentData
    {
        public class _ : IAbilityActivateConditionConverter
        {
            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity) { ecb.AddComponent<AutoActiveAfterCooldownTag>(index, entity); }
        }
    }
}