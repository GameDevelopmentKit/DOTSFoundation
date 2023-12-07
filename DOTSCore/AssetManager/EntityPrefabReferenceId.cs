namespace DOTSCore.AssetManager
{
    using Unity.Collections;
    using Unity.Entities;

    public struct EntityPrefabReferenceId : IComponentData
    {
        public FixedString64Bytes Value;
    }
}