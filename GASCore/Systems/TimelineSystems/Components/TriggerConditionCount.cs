namespace GASCore.Systems.TimelineSystems.Components
{
    using Unity.Entities;

    public struct TriggerConditionCount : IComponentData, IEnableableComponent
    {
        public int Value;
    }
}