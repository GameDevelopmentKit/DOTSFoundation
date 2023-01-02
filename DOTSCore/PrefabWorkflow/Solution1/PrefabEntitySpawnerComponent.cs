namespace DOTSCore.PrefabWorkflow
{
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;

    public struct PrefabEntitySpawnerComponent : IComponentData
    {
        public FixedString64Bytes PrefabName;
        public float3              Position;
    }

    public class ComponentElement : IComponentData
    {
        public IComponentData[] ComponentData;
    }

    public struct SetParentComponent : IComponentData
    {
        public Entity ParentEntity;
    }

    public struct PrefabSpawnedSignal : IComponentData
    {
        public FixedString64Bytes PrefabName;
        public Entity             PrefabEntity;
    }
}