namespace GASCore.Systems.TimelineSystems.Components
{
    using Unity.Entities;

    public struct TriggerConditionAmount : IComponentData
    {
        public                          int Value;
        public static implicit operator int(TriggerConditionAmount amount) => amount.Value;
        public static implicit operator TriggerConditionAmount(int value)  => new() { Value = value };
    }

    public struct CompletedTriggerElement : IBufferElementData
    {
        public                          int Index;
        public static implicit operator int(CompletedTriggerElement trigger) => trigger.Index;
        public static implicit operator CompletedTriggerElement(int index)   => new() { Index = index };
    }

    public struct CompletedAllTriggerConditionTag : IComponentData, IEnableableComponent
    {
    }
}