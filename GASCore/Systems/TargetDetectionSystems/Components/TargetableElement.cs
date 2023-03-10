namespace GASCore.Systems.TargetDetectionSystems.Components
{
    using Unity.Entities;

    public struct TargetableElement : IBufferElementData
    {
        public                          Entity Value;
        public static implicit operator Entity(TargetableElement target) => target.Value;
        public static implicit operator TargetableElement(Entity entity) => new() { Value = entity };
    }
}