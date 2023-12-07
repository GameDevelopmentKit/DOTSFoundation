namespace GASCore.Systems.LogicEffectSystems.Components
{
    using Unity.Entities;

    public struct Duration : IComponentData, IEnableableComponent
    {
        public float Value;
    }
}