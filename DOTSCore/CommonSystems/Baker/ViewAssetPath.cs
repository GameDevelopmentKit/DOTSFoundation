namespace DOTSCore.CommonSystems.Baker
{
    using DOTSCore.AssetManager;
    using DOTSCore.CommonSystems.Components;
    using Unity.Collections;
    using Unity.Entities;
    using UnityEngine;

    [DisallowMultipleComponent]
    public class ViewAssetPath : MonoBehaviour
    {
        public string AddressablePath;
    }

    public class ViewAssetPathBaker : Baker<ViewAssetPath>
    {
        public override void Bake(ViewAssetPath authoring) { this.AddComponent(this.GetEntity(TransformUsageFlags.Dynamic), new AddressablePathComponent() { Value = authoring.AddressablePath }); }
    }

    public static class ViewAssetPathExtensions
    {
        public static void AddViewAssetPathComponent(this EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity, FixedString64Bytes prefabAssetName, bool isUsingAddressable)
        {
#if UNITY_WEBGL
            isUsingAddressable = true;
#endif
            if (isUsingAddressable)
            {
                ecb.AddComponent(index, entity, new AddressablePathComponent() { Value = prefabAssetName });
            }
            else
            {
                ecb.AddComponent(index, entity, new EntityPrefabReferenceId() { Value = prefabAssetName });
            }
        }

        public static void AddViewAssetPathComponent(this EntityManager entityManager, Entity entity, FixedString64Bytes prefabAssetName, bool isUsingAddressable)
        {
#if UNITY_WEBGL
            isUsingAddressable = true;
#endif
            if (isUsingAddressable)
            {
                entityManager.AddComponentData(entity, new AddressablePathComponent() { Value = prefabAssetName });
            }
            else
            {
                entityManager.AddComponentData(entity, new EntityPrefabReferenceId() { Value = prefabAssetName });
            }
        }
    }
}