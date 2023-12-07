namespace GASCore.Systems.LogicEffectSystems.Components
{
    using Unity.Collections;
    using Unity.Entities;

    public struct ItemId : IComponentData
    {
        public FixedString64Bytes Id;
    }
}