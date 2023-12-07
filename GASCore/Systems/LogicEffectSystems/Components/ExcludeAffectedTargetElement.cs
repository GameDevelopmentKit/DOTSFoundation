namespace GASCore.Systems.LogicEffectSystems.Components
{
    using Unity.Entities;

    public struct ExcludeAffectedTargetElement : IBufferElementData
    {
        public Entity Value;
    }
}