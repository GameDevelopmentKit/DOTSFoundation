namespace DOTSCore.CommonSystems.Components
{
    using Unity.Entities;

    public struct AnimationTriggerComponent : IComponentData
    {
        public AnimationTrigger Value;

        public void ChangeState(AnimationTrigger nextTrigger)
        {
            this.Value     = nextTrigger;
        }
    }

    public enum AnimationTrigger
    {
        AttackA,
        AttackB,
        Damaged,
        Victory,
        Dead,
        Idle,
    }
}