namespace GASCore.Systems.VisualEffectSystems.Components
{
    using GASCore.Interfaces;
    using Unity.Entities;
    using Unity.Mathematics;

    public struct CurveForward : IComponentData
    {
        public float Distance;
        public float MoveSpeed;
        public float RotateSpeed;
        public float RemainingRotateAngle;

        public float3 Destination;
        public int    Clockwise;

        public class _ : IAbilityActionComponentConverter
        {
            public float Distance              = 10f;
            public float MoveSpeedMultiplier   = 1f;
            public float RotateSpeedMultiplier = 1f;
            public float CurveAngleDegree      = 90f;

            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
            {
                ecb.AddComponent(index, entity, new CurveForward
                {
                    Distance             = this.Distance,
                    MoveSpeed            = 10f * this.MoveSpeedMultiplier,
                    RotateSpeed          = 2 * math.PI * this.RotateSpeedMultiplier,
                    RemainingRotateAngle = math.radians(this.CurveAngleDegree),
                    Clockwise            = 0,
                });
            }
        }
    }
}