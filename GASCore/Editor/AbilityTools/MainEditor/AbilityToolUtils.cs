namespace GASCore.Editor.AbilityTools.MainEditor
{
    using System.Collections.Generic;
    using System.IO;
    using GASCore.Editor.GoogleSheetSync;
    using GASCore.Editor.ScriptableObject;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    public static class AbilityToolUtils
    {
        public const string AbilityItemSOFolderPath = "Assets/ScriptableObjects/AbilityItems/";
        public const string AbilityToolEditorPath   = "Assets/DOTSFoundation/GASCore/Editor/AbilityTools";
        public const string PathGoogleSheetConfig   = "Assets/DOTSFoundation/GASCore/Editor/GoogleSheetSync/ConfigGoogleSheet.asset";

        private static readonly string AbilityWindowEditorPath   = $"{AbilityToolEditorPath}/{nameof(MainEditor)}/AbilityWindowEditor.uxml";
        private static readonly string AbilityItemRowEditorPath  = $"{AbilityToolEditorPath}/ItemRowEditor/AbilityItemRow.uxml";
        private static readonly string AbilityItemInfoEditorPath = $"{AbilityToolEditorPath}/{nameof(ItemInfoEditor)}/ItemInfoEditor.uxml";
        private static readonly string AbilityLevelItemEditorPath = $"{AbilityToolEditorPath}/{nameof(ItemInfoEditor)}/AbilityLevelItemEditor.uxml";

        public static IEnumerable<T> LoadAllItemAtPath<T>(string folderPath) where T : Object
        {
            var allPaths = Directory.GetFiles(folderPath, "*.asset", SearchOption.AllDirectories);
            foreach (var path in allPaths)
            {
                var cleanedPath = path.Replace("\\", "/");
                yield return AssetDatabase.LoadAssetAtPath<T>(cleanedPath);
            }
        }
        public static VisualTreeAsset LoadAbilityWindowEditor()   => AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AbilityWindowEditorPath);
        public static VisualTreeAsset LoadAbilityItemRowEditor()  => AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AbilityItemRowEditorPath);
        public static VisualTreeAsset LoadAbilityItemInfoEditor() => AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AbilityItemInfoEditorPath);
        public static VisualTreeAsset LoadAbilityLevelItemEditor() => AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AbilityLevelItemEditorPath);

        public static GoogleSheetConfig LoadConfigGoogleSheet() => AssetDatabase.LoadAssetAtPath<GoogleSheetConfig>(PathGoogleSheetConfig);

        public static IEnumerable<AbilityItem> LoadAllAbilityItemSO() => LoadAllItemAtPath<AbilityItem>(AbilityItemSOFolderPath);
    }
}