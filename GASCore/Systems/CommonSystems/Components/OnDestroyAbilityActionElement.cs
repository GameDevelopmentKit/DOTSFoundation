namespace GASCore.Systems.CommonSystems.Components
{
    using Unity.Entities;

    public struct OnDestroyAbilityActionElement : IBufferElementData
    {
        public Entity AbilityActionEntity;
    }
}