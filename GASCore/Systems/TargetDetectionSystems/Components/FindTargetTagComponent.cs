namespace GASCore.Systems.TargetDetectionSystems.Components
{
    using Unity.Entities;

    public struct FindTargetTagComponent : IComponentData, IEnableableComponent
    {
        public bool WaitForOtherTriggers;
    }
}