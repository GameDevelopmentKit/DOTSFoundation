namespace GASCore.Systems.VisualEffectSystems.Components
{
    using GASCore.Interfaces;
    using Sirenix.OdinInspector;
    using Unity.Entities;
    using Unity.Mathematics;

    public enum RotateDirection
    {
        Clockwise        = 1,
        CounterClockwise = -1,
        Random           = 0,
    }

    public struct RotateMovementDirection : IComponentData, IEnableableComponent
    {
        public float RotateInterval;
        public float RotateTimer;
        public float RotateSpeed;
        public int   RotateCount;
        public int   RotateDirection;

        public class _ : IAbilityActionComponentConverter
        {
            public                     float           RotateIntervalSecond = 0.1f;
            public                     float           RotateAngleDegree    = 1f;
            public                     int             RotateCount          = -1;
            [EnumToggleButtons] public RotateDirection RotateDirection      = RotateDirection.Random;

            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
            {
                ecb.AddComponent(index, entity, new RotateMovementDirection
                {
                    RotateInterval  = this.RotateIntervalSecond,
                    RotateTimer     = this.RotateIntervalSecond,
                    RotateSpeed     = math.radians(this.RotateAngleDegree),
                    RotateCount     = this.RotateCount,
                    RotateDirection = (int)this.RotateDirection,
                });
            }
        }
    }
}