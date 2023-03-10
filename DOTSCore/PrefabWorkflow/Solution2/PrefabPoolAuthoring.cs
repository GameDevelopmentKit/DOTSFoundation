namespace DOTSCore.PrefabWorkflow
{
    using System.Collections.Generic;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;
    using UnityEngine;

    public class PrefabPoolAuthoring : MonoBehaviour
    {
        public PrefabPoolData PoolData;

        public class PrefabPoolBaker : Baker<PrefabPoolAuthoring>
        {
            public override void Bake(PrefabPoolAuthoring authoring)
            {
                this.AddComponent<LocalTransform>();

                if (authoring.PoolData == null || authoring.PoolData.ListPrefab == null || authoring.PoolData.ListPrefab.Count == 0)
                    return;

                var prefabsDeduped = new HashSet<Entity>();

                foreach (var prefab in authoring.PoolData.ListPrefab)
                {
                    var prefabEntity = this.GetEntity(prefab);

                    if (prefabEntity == Entity.Null) continue;

                    prefabsDeduped.Add(prefabEntity);
                }

                var groupBuffer = this.AddBuffer<PrefabPool>();
                foreach (var prefabEntity in prefabsDeduped)
                    groupBuffer.Add(new PrefabPool() { Value = prefabEntity });
            }
        }
    }

    public struct PrefabPool : IBufferElementData
    {
        public Entity Value;
    }
}