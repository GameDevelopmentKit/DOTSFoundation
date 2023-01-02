namespace DOTSCore.PrefabWorkflow
{
    using System.Collections.Generic;
    using System.IO;
    using Sirenix.OdinInspector;
    using UnityEngine;

    [CreateAssetMenu(order = 0, fileName = nameof(PrefabDatabase), menuName = "DOTSCore/Create PrefabPoolData")]
    public class PrefabPoolData : SerializedScriptableObject
    {
        [FolderPath] [SerializeField] private string[] prefabDirectoryPaths;

        [ReadOnly] [SerializeField] private List<GameObject> listPrefab;

        public List<GameObject> ListPrefab => this.listPrefab;
#if UNITY_EDITOR

        private void OnValidate()
        {
            this.listPrefab = new List<GameObject>();
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
                    var fullPath  = fileInfo.FullName.Replace(@"\", "/");
                    var assetPath = "Assets" + fullPath.Replace(Application.dataPath, "");
                    this.listPrefab.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(assetPath));
                }
            }
        }

        [Button]
        public void Refresh() { this.OnValidate(); }
#endif
    }
}