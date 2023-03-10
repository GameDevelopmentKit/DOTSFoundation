namespace GASCore.Systems.VisualEffectSystems.Components
{
    using Unity.Entities;
    using GASCore.Interfaces;

    public struct Spinnable : IComponentData
    {
        // unit = radians/second
        // if current SpinRate >= MaxSpinRate --> inverts spin

        public float SpinRate;
        public float SpinModificator;
        public float MaxSpinRate;
        public int   InvertSpin;
        public int   ClockWise;

        public class _ : IAbilityActionComponentConverter
        {
            public float SpinRate        = 1f;
            public float SpinModificator = 0.1f;
            public float MaxSpinRate     = 1f;
            public bool  IsClockWise     = true;

            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
            {
                ecb.AddComponent(index, entity, new Spinnable()
                {
                    SpinRate        = this.SpinRate,
                    SpinModificator = this.SpinModificator,
                    MaxSpinRate     = this.MaxSpinRate,
                    ClockWise       = this.IsClockWise ? 1 : -1,
                    InvertSpin      = 1,
                });
            }
        }
    }
}