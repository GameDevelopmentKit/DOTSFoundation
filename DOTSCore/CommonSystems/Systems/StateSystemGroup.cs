namespace DOTSCore.Group
{
    using DOTSCore.CommonSystems.Components;
    using Unity.Collections;
    using Unity.Entities;

    //todo upgrade state system group to more independent
    public abstract partial class StateSystemGroup : ComponentSystemGroup
    {
        protected virtual NativeHashSet<FixedString64Bytes> RunInStates { get; } = new(0, Allocator.Persistent);
        protected override void OnCreate()
        {
            base.OnCreate();
            this.RequireForUpdate<CurrentGameState>();
        }

        protected override void OnUpdate()
        {
            if (this.RunInStates.Count == 0 || this.RunInStates.Contains(SystemAPI.GetSingleton<CurrentGameState>().Value))
            {
                base.OnUpdate();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (this.RunInStates.IsCreated)
            {
                this.RunInStates.Dispose();
            }
        }
    }
}