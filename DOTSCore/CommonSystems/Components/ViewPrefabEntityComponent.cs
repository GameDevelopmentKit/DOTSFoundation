namespace DOTSCore.CommonSystems.Components
{
    using Unity.Collections;
    using Unity.Entities;

    public struct ViewPrefabEntityComponent : IComponentData
    {
        public FixedString64Bytes Value;
    }
}