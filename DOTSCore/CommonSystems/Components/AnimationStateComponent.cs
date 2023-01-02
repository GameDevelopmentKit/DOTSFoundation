namespace DOTSCore.CommonSystems.Components
{
    using Unity.Entities;

    public struct AnimationStateComponent : IComponentData
    {
        public AnimationState Value;
        public bool           IsChangeState;

        public void ChangeState(AnimationState nextState)
        {
            if (nextState == this.Value) return;
            this.Value     = nextState;
            this.IsChangeState = true;
        }
    }

    public enum AnimationState
    {
        Idle,
        Move,
        Dead,
        AttackA,
        Damaged
    }
}