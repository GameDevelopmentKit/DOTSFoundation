namespace GASCore.Systems.CasterSystems.Components
{
    using Unity.Collections;
    using Unity.Entities;

    public struct AbilityContainerElement : IBufferElementData
    {
        public FixedString64Bytes AbilityId;
        public int                Level;
        public Entity             AbilityInstance;
    }
}