#if UNITY_EDITOR && !UNITY_WEBGL
namespace DOTSCore.AssetManager
{
    using System.Collections.Generic;
    using Unity.Entities;
    using Unity.Entities.Serialization;
    using UnityEngine;

    [DisallowMultipleComponent]
    public class EntityPrefabPoolAuthoring : MonoBehaviour
    {
        public List<GameObject>    ListPrefab;
    }

    public class PrefabPoolBaker : Baker<EntityPrefabPoolAuthoring>
    {
        public override void Bake(EntityPrefabPoolAuthoring authoring)
        {
            if (authoring.ListPrefab == null ||  authoring.ListPrefab.Count == 0) return;

            var hashsetPrefab = new HashSet<GameObject>(authoring.ListPrefab);
            var prefabBuffer  = this.AddBuffer<EntityPrefabReferenceElement>(this.GetEntity(TransformUsageFlags.Dynamic));

            foreach (var prefab in hashsetPrefab)
            {
                prefabBuffer.Add(new EntityPrefabReferenceElement()
                {
                    Prefab     = new EntityPrefabReference(prefab),
                    PrefabName = prefab.name
                });
            }
        }
    }
}
#endif

