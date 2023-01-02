namespace GASCore.Systems.CommonSystems.Components
{
    using Unity.Entities;

    /// <summary>
    /// The end time in millisecond is calculated from epoch time, sync from server to client
    /// </summary>
    public struct EndTimeComponent : IComponentData, IEnableableComponent
    {
        public double Value;
    }
}