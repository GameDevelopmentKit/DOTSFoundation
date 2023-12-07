namespace TutorialSystem.Helper {
    using System.Collections.Generic;
    using UnityEngine;

    public class TutorialObjectCollection {  
        private static readonly Dictionary<string, GameObject> ObjectCollectionDic = new();

        public static GameObject GetObjectInstanceByPath(string path)
        {
            GetObjectInstanceByPath(path, out var existingObject);
            return existingObject;
        }
        public static bool GetObjectInstanceByPath(string path, out GameObject existingObject) {
            if (ObjectCollectionDic.TryGetValue(path, out existingObject) && existingObject != null) {
                return true;
            }

            existingObject = GameObject.Find(path);
            if (existingObject != null) {
                ObjectCollectionDic[path] = existingObject;
                return true;
            }

            Debug.LogError($"Don't find {path} object in scene");
            return false;
        }
        
        public static void Clear() {
            ObjectCollectionDic.Clear();
        }
    }
}
