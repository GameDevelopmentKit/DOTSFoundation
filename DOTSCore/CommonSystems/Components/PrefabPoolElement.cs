namespace DOTSCore.CommonSystems.Components
{
    using System;
    using Unity.Collections;
    using Unity.Entities;

    [Serializable]
    public struct PrefabPoolElement : IBufferElementData
    {
        public FixedString64Bytes PrefabName;
        public Entity             Prefab;
    }
}