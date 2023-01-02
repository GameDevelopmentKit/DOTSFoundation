namespace DOTSCore.CommonSystems.Components
{
    using Unity.Collections;
    using Unity.Entities;

    public struct CurrentGameState : IComponentData
    {
        public FixedString64Bytes Value;
    }

    public struct PreviousGameState : IComponentData
    {
        public FixedString64Bytes Value;
    }

    public struct RequestChangeGameState : IComponentData, IEnableableComponent
    {
        public bool               IsForce;
        public FixedString64Bytes NextState;
    }

}