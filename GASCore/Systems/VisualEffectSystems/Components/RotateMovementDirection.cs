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
        public float IdleInterval;
        public float RotateInterval;
        public float RotateSpeed;
        public int   RotateCount;
        public int   RotateDirection;

        public float RotateTimer;
        public float IdleTimer;

        public class _ : IAbilityActionComponentConverter
        {
            [PropertyTooltip("idle time before rotating, in second")]
            public float IdleInterval;

            public                     float           RotateIntervalSecond = 0.1f;
            public                     float           RotateAngleDegree    = 1f;
            public                     int             RotateCount          = -1;
            [EnumToggleButtons] public RotateDirection RotateDirection      = RotateDirection.Random;

            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
            {
                ecb.AddComponent(index, entity, new RotateMovementDirection
                {
                    IdleInterval    = this.IdleInterval,
                    IdleTimer       = this.IdleInterval,
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