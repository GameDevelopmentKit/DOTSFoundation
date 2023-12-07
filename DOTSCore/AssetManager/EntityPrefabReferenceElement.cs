namespace DOTSCore.AssetManager
{
    using System;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Entities.Serialization;

    [Serializable]
    public struct EntityPrefabReferenceElement : IBufferElementData
    {
        public FixedString64Bytes    PrefabName;
        public EntityPrefabReference Prefab;
    }
}