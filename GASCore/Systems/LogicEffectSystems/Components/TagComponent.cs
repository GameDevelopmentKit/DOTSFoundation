namespace GASCore.Systems.LogicEffectSystems.Components
{
    using Unity.Collections;
    using Unity.Entities;

    public struct TagComponent : IComponentData
    {
        public FixedString64Bytes Value;
    }
}