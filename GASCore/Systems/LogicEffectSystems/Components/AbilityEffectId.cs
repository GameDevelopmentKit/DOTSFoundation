namespace GASCore.Systems.LogicEffectSystems.Components
{
    using Unity.Collections;
    using Unity.Entities;

    public struct AbilityEffectId : IComponentData
    {
        public FixedString64Bytes Value;
    }
}