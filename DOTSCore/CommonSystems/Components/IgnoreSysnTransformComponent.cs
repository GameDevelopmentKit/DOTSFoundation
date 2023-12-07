namespace DOTSCore.CommonSystems.Components
{
    using Unity.Entities;

    public struct IgnoreSysnTransformComponent : IComponentData
    {
        public bool IsAddForAffectTarget;
    }
}