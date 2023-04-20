namespace GASCore.Systems.LogicEffectSystems.Components
{
    using Unity.Entities;
    using Unity.Mathematics;

    public struct TargetPosition : IComponentData
    {
        public float3 Value;
        public float  RadiusSq;
        public TargetPosition(float3 value, float radiusSq = 0.01f)
        {
            this.Value              = value;
            this.RadiusSq           = radiusSq;
        }
    }

    public struct ChaseTargetEntity : IComponentData
    {
        public Entity Value;
        public bool3  LockAxis;
    }
}