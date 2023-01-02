namespace DOTSCore.CommonSystems.Components
{
    using Unity.Collections;
    using Unity.Entities;

    public struct AssetPathComponent : IComponentData
    {
        public FixedString128Bytes Path;
    }
}