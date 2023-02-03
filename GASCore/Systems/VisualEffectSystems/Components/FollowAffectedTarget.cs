namespace GASCore.Systems.VisualEffectSystems.Components
{
    using GASCore.Interfaces;
    using GASCore.Systems.LogicEffectSystems.Components;
    using Unity.Entities;
    using Unity.Mathematics;

    public struct FollowAffectedTarget : IComponentData
    {
        public float Radius;
        public float MoveSpeed;
        public bool3 LockAxis;

        public class _ : IAbilityActionComponentConverter
        {
            public float       Radius              = 1f;
            public float       MoveSpeedMultiplier = 1f;
            public SimpleBool3 LockAxis            = new(false, true, false);

            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
            {
                ecb.AddComponent(index, entity, new FollowAffectedTarget
                {
                    Radius    = this.Radius,
                    MoveSpeed = 10f * this.MoveSpeedMultiplier,
                    LockAxis  = this.LockAxis,
                });
            }
        }
    }
}