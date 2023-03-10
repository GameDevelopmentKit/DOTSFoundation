namespace GASCore.Systems.LogicEffectSystems.Components
{
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    public struct EnemyTag : IComponentData
    {
    }

    public struct SpawnData : IBufferElementData
    {
        public Entity Enemy;
        public float  Time;
        public int    Count;
        public float  Range;
    }

    public struct Anchor : IComponentData
    {
        public                          float3 Position;
        public static implicit operator float3(Anchor anchor)              => anchor.Position;
        public static implicit operator LocalTransform(Anchor anchor)      => new() { Position = anchor };
        public static implicit operator Anchor(float3 position)            => new() { Position = position };
        public static implicit operator Anchor(LocalTransform translation) => new() { Position = translation.Position };
        public static implicit operator Anchor(LocalToWorld localToWorld)  => new() { Position = localToWorld.Position };
    }

    public struct DynamicAnchor : IComponentData
    {
        public                          Entity Entity;
        public static implicit operator Entity(DynamicAnchor anchor) => anchor.Entity;
        public static implicit operator DynamicAnchor(Entity entity) => new() { Entity = entity };
    }
}