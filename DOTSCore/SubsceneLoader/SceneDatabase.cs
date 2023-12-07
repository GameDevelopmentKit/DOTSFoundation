namespace DOTSCore.SubsceneLoader
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Sirenix.OdinInspector;
    using Unity.Collections;
    using UnityEngine;
    using Hash128 = Unity.Entities.Hash128;

    /// <summary>
    /// This is used when the map is required inside a job
    /// </summary>
    public struct EntityPrefabGuidResolver
    {
        private NativeParallelHashMap<FixedString128Bytes, Hash128> map;

        public EntityPrefabGuidResolver(NativeParallelHashMap<FixedString128Bytes, Hash128> map) { this.map = map; }

        public Hash128 GetPrefabEntityGuid(FixedString128Bytes id)
        {
            if (this.map.TryGetValue(id, out var prefabEntityGuid))
            {
                return prefabEntityGuid;
            }

            throw new Exception($"The prefab pool does not contain an entry for {id}");
        }
    }

    [CreateAssetMenu(order = 0, fileName = nameof(SceneDatabase), menuName = "DOTSCore/Create SceneDatabase")]
    public class SceneDatabase : SerializedScriptableObject
    {
#if UNITY_EDITOR
        [InlineButton("OnValidate", SdfIconType.Recycle, "Refresh")] [FolderPath] [SerializeField]
        private string[] sceneDirectoryPaths;

        private void OnValidate()
        {
            this.sceneInfos = new List<SceneItem>();
            foreach (var path in this.sceneDirectoryPaths)
            {
                var correctPath = path;
                if (path != "" && path.EndsWith("/"))
                {
                    correctPath = path.TrimEnd('/');
                }

                var dirInfo = new DirectoryInfo(correctPath);
                var fileInf = dirInfo.GetFiles("*.unity", SearchOption.AllDirectories);

                foreach (var fileInfo in fileInf)
                {
                    var fullPath  = fileInfo.FullName.Replace(@"\", "/");
                    var assetPath = "Assets" + fullPath.Replace(Application.dataPath, "");
                    var sceneGuid = UnityEditor.AssetDatabase.AssetPathToGUID(assetPath);

                    if (string.IsNullOrEmpty(sceneGuid)) continue;
                    this.sceneInfos.Add(new SceneItem()
                    {
                        SceneName = Path.GetFileNameWithoutExtension(fileInfo.Name),
                        Guid      = sceneGuid,
                    });
                }
            }
        }
#endif

        private class SceneItem
        {
            public string SceneName;
            public string Guid;
        }

        [SerializeField] [Sirenix.OdinInspector.ReadOnly]
        private List<SceneItem> sceneInfos;

        #region ecs handler

        private NativeParallelHashMap<FixedString128Bytes, Hash128> sceneNameToGuid;

        public Hash128 GetGuidFromSceneName(string prefabName)
        {
            if (this.sceneNameToGuid.IsEmpty)
            {
                this.sceneNameToGuid = new NativeParallelHashMap<FixedString128Bytes, Hash128>(this.sceneInfos.Count, Allocator.Persistent);
                foreach (var sceneItem in this.sceneInfos)
                {
                    this.sceneNameToGuid.Add(sceneItem.SceneName, new Hash128(sceneItem.Guid));
                }
            }

            if (this.sceneNameToGuid.TryGetValue(prefabName, out var prefabEntityGuid))
            {
                return prefabEntityGuid;
            }

            throw new Exception($"The prefab pool does not contain an entry for {prefabName}");
        }

        public EntityPrefabGuidResolver SceneGuidResolver => new EntityPrefabGuidResolver(this.sceneNameToGuid);

        #endregion
    }
}