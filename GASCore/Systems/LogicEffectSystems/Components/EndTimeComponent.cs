namespace GASCore.Systems.LogicEffectSystems.Components
{
    using Unity.Entities;

    /// <summary>
    /// The end time in millisecond is calculated from epoch time, sync from server to client
    /// </summary>
    public struct EndTimeComponent : IComponentData, IEnableableComponent
    {
        public float AmountTime;
        public double NextEndTimeValue;
    }
}