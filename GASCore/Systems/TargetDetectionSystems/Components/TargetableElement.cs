namespace GASCore.Systems.TargetDetectionSystems.Components
{
    using Unity.Entities;

    public struct TargetableElement : IBufferElementData
    {
        public Entity Value;
    }
}