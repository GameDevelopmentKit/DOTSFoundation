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
    public struct EntityPrefabGuidResolver {
        private NativeParallelHashMap<FixedString128Bytes, Hash128> map;
 
        public EntityPrefabGuidResolver(NativeParallelHashMap<FixedString128Bytes, Hash128> map) {
            this.map = map;
        }
 
        public Hash128 GetPrefabEntityGuid(FixedString128Bytes id) {
            if (this.map.TryGetValue(id, out var prefabEntityGuid)) {
                return prefabEntityGuid;
            }
         
            throw new Exception($"The prefab pool does not contain an entry for {id}");
        }
    }

    [CreateAssetMenu(order = 0, fileName = nameof(SceneDatabase), menuName = "DOTSCore/Create SceneDatabase")]
    public class SceneDatabase : SerializedScriptableObject
    {

        private NativeParallelHashMap<FixedString128Bytes, Hash128> sceneNameToGuid;

        public Hash128 GetGuidFromSceneName(string prefabName)
        {
            if (this.sceneNameToGuid.TryGetValue(prefabName, out var prefabEntityGuid)) {
                return prefabEntityGuid;
            }
         
            throw new Exception($"The prefab pool does not contain an entry for {prefabName}");
        }
        
        public EntityPrefabGuidResolver SceneGuidResolver => new EntityPrefabGuidResolver(this.sceneNameToGuid);

#if UNITY_EDITOR
        [FolderPath] [SerializeField] private string[] sceneDirectoryPaths;
        [SerializeField][Sirenix.OdinInspector.ReadOnly] private Dictionary<UnityEditor.SceneAsset, string> SceneToGuidViewer;
        private void OnValidate()
        {
           
            this.SceneToGuidViewer = new Dictionary<UnityEditor.SceneAsset, string>();
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
                    var fullPath   = fileInfo.FullName.Replace(@"\", "/");
                    var assetPath  = "Assets" + fullPath.Replace(Application.dataPath, "");
                    var sceneGuid = UnityEditor.AssetDatabase.AssetPathToGUID(assetPath);

                    if (string.IsNullOrEmpty(sceneGuid)) continue;
                    this.SceneToGuidViewer.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.SceneAsset>(assetPath), sceneGuid);
                }
            }

            if (this.sceneNameToGuid.IsCreated)
            {
                this.sceneNameToGuid.Dispose();
            }
            this.sceneNameToGuid = new NativeParallelHashMap<FixedString128Bytes, Hash128>(this.SceneToGuidViewer.Count, Allocator.Persistent);
            foreach (var (prefab, guid) in this.SceneToGuidViewer)
            {
                this.sceneNameToGuid.Add(prefab.name, new Hash128(guid));
            }
        }

        [Button]
        public void Refresh()
        {
            this.OnValidate();
        }
#endif
    }
}
