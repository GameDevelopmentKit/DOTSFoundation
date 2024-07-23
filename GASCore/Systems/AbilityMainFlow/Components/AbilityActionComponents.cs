namespace GASCore.Systems.AbilityMainFlow.Components
{
    using Unity.Entities;

    public struct AffectedTargetComponent : IComponentData
    {
        public                          Entity Value;
        public static implicit operator Entity(AffectedTargetComponent target) => target.Value;
        public static implicit operator AffectedTargetComponent(Entity target) => new() { Value = target };
    }

    public struct CasterComponent : IComponentData
    {
        public                          Entity Value;
        public static implicit operator Entity(CasterComponent caster) => caster.Value;
        public static implicit operator CasterComponent(Entity caster) => new() { Value = caster };
    }

    public struct AimingComponent : IComponentData
    {
        public                          Entity Value;
        public static implicit operator Entity(AimingComponent aiming) => aiming.Value;
        public static implicit operator AimingComponent(Entity aiming) => new() { Value = aiming };
    }

    public struct AbilityOwner : IComponentData
    {
        public Entity Value;
    }

    public struct ActivatedStateEntityOwner : ICleanupComponentData
    {
        public                          Entity Value;
        public static implicit operator Entity(ActivatedStateEntityOwner owner) => owner.Value;
        public static implicit operator ActivatedStateEntityOwner(Entity owner) => new() { Value = owner };
    }


}