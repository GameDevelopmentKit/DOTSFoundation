namespace GASCore.Systems.LogicEffectSystems.Components
{
    using GASCore.Blueprints;
    using Unity.Entities;

    public struct AffectedTargetTypeElement : IBufferElementData
    {
        public TargetType Value;
    }
}