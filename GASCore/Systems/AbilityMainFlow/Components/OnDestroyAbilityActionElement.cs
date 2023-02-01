namespace GASCore.Systems.AbilityMainFlow.Components
{
    using Unity.Entities;

    public struct OnDestroyAbilityActionElement : IBufferElementData
    {
        public Entity AbilityActionEntity;
    }
}