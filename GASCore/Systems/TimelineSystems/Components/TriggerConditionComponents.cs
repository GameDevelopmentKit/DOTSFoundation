namespace GASCore.Systems.TimelineSystems.Components
{
    using Unity.Entities;

    public struct TriggerConditionAmount : IComponentData
    {
        public int Value;
    }

    public struct CompletedTriggerElement : IBufferElementData
    {
        public int Index;
    }

    public struct CompletedAllTriggerConditionTag : IComponentData, IEnableableComponent { }
}