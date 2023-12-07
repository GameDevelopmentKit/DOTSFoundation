#if UNITY_EDITOR
namespace TutorialSystem.Editor
{
    using UnityEditor;
    using UnityEngine;

    public static class TutorialEditor
    {
        [MenuItem("GameObject/Tutorial/Copy Direct Path", false, 0)]
        static void CopyDirectPath(MenuCommand menuCommand)
        {
            string fullPathStr = GetGameObjectPath(Selection.activeTransform);
            EditorGUIUtility.systemCopyBuffer = fullPathStr;
            Debug.Log($"Direct path {fullPathStr} was copied");
        }

        private static string GetGameObjectPath(Transform transform)
        {
            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path      = transform.name + "/" + path;
            }

            return path;
        }
    }
}

#endif