namespace GASCore.Systems.LogicEffectSystems.Components
{
    using Unity.Entities;
    public struct NeedToTrackingTargetInCastRange : IComponentData { }

    public struct EntityInAbilityRangeElement : IBufferElementData
    {
        public Entity Value;
    }
    
    public struct OnInAbilityRange : IComponentData
    {
        public Entity TargetEntity;
        public Entity ActivatedStateAbilityEntity;
    }
    
    public struct OnOutAbilityRange : IComponentData
    {
        public Entity TargetEntity;
        public Entity ActivatedStateAbilityEntity;
    }
}