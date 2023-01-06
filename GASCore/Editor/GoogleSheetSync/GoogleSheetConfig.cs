namespace GASCore.Editor.GoogleSheetSync
{
    using Sirenix.OdinInspector;
    using UnityEngine;
    using ScriptableObject = UnityEngine.ScriptableObject;

    [CreateAssetMenu(fileName = "ConfigGoogleSheet", menuName = "GDK/ConfigGoogleSheet")]
    public class GoogleSheetConfig : ScriptableObject
    {
        public string SpreadSheetID;
    
        [Header("Local Resource")]
        public bool IsLocalResource;

        [FolderPath]
        public string FolderPath;
        public string LocalSheetPath => $"{this.FolderPath}/{this.SpreadSheetID}.csv";
   
        [Header("Remote Spread Sheet")]
        public string WorkSheetName;
        public string UploadSheetName;
        public string UrlExportSpreadSheetCSV    => $"https://docs.google.com/spreadsheets/d/{this.SpreadSheetID}/export?format=csv";
        public string UrlExportSpecificWorkSheet => $"https://docs.google.com/spreadsheets/d/{this.SpreadSheetID}/gviz/tq?tqx=out:csv&sheet={this.WorkSheetName}";
    }
}