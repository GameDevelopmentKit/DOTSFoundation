namespace GASCore.Systems.AbilityMainFlow.Components
{
    using Unity.Entities;

    public struct TargetableElement : IBufferElementData
    {
        public Entity Value;
    }
    
    public struct AffectedTargetComponent : IComponentData
    {
        public Entity Value;
    }
    
    public struct CasterComponent : IComponentData
    {
        public Entity Value;
    }

    public struct AbilityOwner : IComponentData
    {
        public Entity Value;
    }
    
    public struct ActivatedStateEntityOwner : IComponentData
    {
        public Entity Value;
    }
}