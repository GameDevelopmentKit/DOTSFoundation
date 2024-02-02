namespace GASCore.Systems.TargetDetectionSystems.Components
{
    using Unity.Entities;

    [InternalBufferCapacity(0)]
    public struct TargetableElement : IBufferElementData
    {
        public                          Entity Value;
        public static implicit operator Entity(TargetableElement target) => target.Value;
        public static implicit operator TargetableElement(Entity entity) => new() { Value = entity };
    }
}