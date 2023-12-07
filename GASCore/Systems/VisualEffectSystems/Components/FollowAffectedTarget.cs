namespace GASCore.Systems.VisualEffectSystems.Components
{
    using GASCore.Interfaces;
    using Unity.Entities;
    using Unity.Mathematics;

    public struct FollowAffectedTarget : IComponentData
    {
        public bool  ReverseOrder;
        public float Radius;
        public bool3 LockAxis;

        public class _ : IAbilityActionComponentConverter
        {
            public bool        ReverseOrder = false;
            public float       Radius       = 1f;
            public SimpleBool3 LockAxis     = new(false, true, false);

            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
            {
                ecb.AddComponent(index, entity, new FollowAffectedTarget
                {
                    ReverseOrder = this.ReverseOrder,
                    Radius       = this.Radius,
                    LockAxis     = this.LockAxis,
                });
            }
        }
    }
}