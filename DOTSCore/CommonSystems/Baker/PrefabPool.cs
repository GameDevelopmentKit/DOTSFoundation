namespace DOTSCore.CommonSystems.Baker
{
    using System.Collections.Generic;
    using DOTSCore.CommonSystems.Components;
    using Unity.Entities;
    using UnityEngine;

    [DisallowMultipleComponent]
    public class PrefabPool : MonoBehaviour
    {
        public List<GameObject> ListPrefab;
    }

    public class PrefabPoolBaker : Baker<PrefabPool>
    {
        public override void Bake(PrefabPool authoring)
        {
            if (authoring.ListPrefab == null ||  authoring.ListPrefab.Count == 0) return;

            var prefabBuffer = this.AddBuffer<PrefabPoolElement>(this.GetEntity(TransformUsageFlags.Dynamic));

            foreach (var prefab in authoring.ListPrefab)
            {
                var prefabEntity = this.GetEntity(prefab, TransformUsageFlags.Dynamic);

                if (prefabEntity == Entity.Null) continue;

                prefabBuffer.Add(new PrefabPoolElement()
                {
                    Prefab     = prefabEntity,
                    PrefabName = prefab.name
                });
            }
        }
    }
}