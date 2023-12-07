namespace GASCore.Systems.LogicEffectSystems.Components
{
    using Unity.Entities;
    using Unity.Mathematics;

    public struct MovementDirection : IComponentData
    {
        public float3 Value;
    }
}