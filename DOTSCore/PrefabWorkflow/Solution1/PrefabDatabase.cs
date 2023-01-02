namespace DOTSCore.PrefabWorkflow
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

    [CreateAssetMenu(order = 0, fileName = nameof(PrefabDatabase), menuName = "DOTSCore/Create PrefabDatabase")]
    public class PrefabDatabase : SerializedScriptableObject
    {
        [FolderPath] [SerializeField] private string[] prefabDirectoryPaths;

        private NativeParallelHashMap<FixedString128Bytes, Hash128> prefabNameToGuid;

        public Hash128 GetGuidFromPrefabName(string prefabName)
        {
            if (this.prefabNameToGuid.TryGetValue(prefabName, out var prefabEntityGuid)) {
                return prefabEntityGuid;
            }
         
            throw new Exception($"The prefab pool does not contain an entry for {prefabName}");
        }
        
        public EntityPrefabGuidResolver EntityPrefabGuidResolver => new EntityPrefabGuidResolver(this.prefabNameToGuid);

#if UNITY_EDITOR
        [Sirenix.OdinInspector.ReadOnly] public Dictionary<GameObject, string> PrefabToGuidViewer;
        private void OnValidate()
        {
           
            this.PrefabToGuidViewer = new Dictionary<GameObject, string>();
            foreach (var path in this.prefabDirectoryPaths)
            {
                var correctPath = path;
                if (path != "" && path.EndsWith("/"))
                {
                    correctPath = path.TrimEnd('/');
                }

                var dirInfo = new DirectoryInfo(correctPath);
                var fileInf = dirInfo.GetFiles("*.prefab", SearchOption.AllDirectories);

                foreach (var fileInfo in fileInf)
                {
                    var fullPath   = fileInfo.FullName.Replace(@"\", "/");
                    var assetPath  = "Assets" + fullPath.Replace(Application.dataPath, "");
                    var prefabGuid = UnityEditor.AssetDatabase.AssetPathToGUID(assetPath);

                    if (string.IsNullOrEmpty(prefabGuid)) continue;
                    this.PrefabToGuidViewer.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(assetPath), prefabGuid);
                }
            }

            if (this.prefabNameToGuid.IsCreated)
            {
                this.prefabNameToGuid.Dispose();
            }
            this.prefabNameToGuid = new NativeParallelHashMap<FixedString128Bytes, Hash128>(this.PrefabToGuidViewer.Count, Allocator.Persistent);
            foreach (var (prefab, guid) in this.PrefabToGuidViewer)
            {
                this.prefabNameToGuid.Add(prefab.name, new Hash128(guid));
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