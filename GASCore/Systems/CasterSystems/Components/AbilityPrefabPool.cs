namespace GASCore.Systems.CasterSystems.Components
{
    using Unity.Collections;
    using Unity.Entities;

    public struct AbilityPrefabPool : IComponentData
    {
        public BlobAssetReference< NativeParallelHashMap<FixedString64Bytes, Entity>> AbilityNameToLevelPrefabs;
    }
}