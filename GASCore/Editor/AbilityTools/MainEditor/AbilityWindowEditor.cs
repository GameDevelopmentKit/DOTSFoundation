namespace GASCore.Editor.AbilityTools.MainEditor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using GASCore.Blueprints;
    using GASCore.Editor.AbilityTools.ItemInfoEditor;
    using GASCore.Editor.GoogleSheetSync;
    using GASCore.Editor.ScriptableObject;
    using GoogleSheetsToUnity.Editor;
    using UnityEditor;
    using UnityEditor.ShortcutManagement;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;
    using Editor = UnityEditor.Editor;
    using ScriptableObject = UnityEngine.ScriptableObject;

    public class AbilityWindowEditor : EditorWindow
    {
        private GoogleSheetConfig configGoogleSheet;

        private List<AbilityItem> abilityInventory;
        private ListView          abilityItemListView;

        private VisualElement  detailPanelView;
        private ItemInfoEditor itemInfoEditor;

        private AbilityItem activeItem;
        private int         lastSelectionItem;

        private Button pushToSheetBtn;

        [MenuItem("GDK/AbilityWindowEditor _Alt+A", priority = 1)]
        [Shortcut("GDK/AbilityWindowEditor", null, KeyCode.A, ShortcutModifiers.Alt)]
        public static void Init()
        {
            AbilityWindowEditor wnd = GetWindow<AbilityWindowEditor>();
            wnd.titleContent = new GUIContent("Ability Tool Editor");
        }

        public void CreateGUI()
        {
            this.configGoogleSheet = AbilityToolUtils.LoadConfigGoogleSheet();
            // Import UXML
            this.rootVisualElement.Add(AbilityToolUtils.LoadAbilityWindowEditor().Instantiate());

            this.detailPanelView = this.rootVisualElement.Q("ItemDetail");
            this.itemInfoEditor  = new ItemInfoEditor();
            this.detailPanelView.Add(this.itemInfoEditor);
            this.itemInfoEditor.OnUpdateAbilityData = this.RebuildListAbilityItemRowView;

            // setup ability list item view
            this.abilityInventory = AbilityToolUtils.LoadAllAbilityItemSO().ToList();
            this.GenerateListView(this.abilityInventory);

            //Register event
            //Hook up button click events
            this.rootVisualElement.Q<VisualElement>("ItemsTab").Q<Button>("btnAdd").clicked += this.AddItem_OnClick;
            this.detailPanelView.Q<Button>("btnDelete").clicked                             += this.DeleteItem_OnClick;

            //Register Value Changed Callbacks for new items added to the ListView
            this.rootVisualElement.Q<Button>("btnPullFromSpreadSheet").clicked += this.PullDataFromSpreadSheet;

            this.pushToSheetBtn                                        =  this.rootVisualElement.Q<Button>("btnPushToSpreadSheet");
            this.pushToSheetBtn.clicked                                += this.PushDataToSpreadSheet;
            this.rootVisualElement.Q<Button>("btnApiConfig").clicked   += this.ApiConfig;
            this.rootVisualElement.Q<Button>("btnSheetConfig").clicked += this.SheetConfig;

            this.rootVisualElement.Q<ToolbarSearchField>("search_field").RegisterValueChangedCallback(this.OnSearchTextChanged);
        }

        private void OnSearchTextChanged(ChangeEvent<string> evt)
        {
            var abilitySearch = new List<AbilityItem>(abilityInventory.Where(abilityItem => abilityItem.name.Contains(evt.newValue, StringComparison.OrdinalIgnoreCase)));
            this.abilityItemListView.itemsSource = abilitySearch;
            this.abilityItemListView.bindItem = async (e, i) =>
            {
                if (abilitySearch[i] == null) return;
                e.Q<Label>("Name").text                          = abilitySearch[i].name;
                e.Q<VisualElement>("Icon").style.backgroundImage = (await abilitySearch[i].Icon.LoadLocalSprite())?.texture;
            };
            this.RebuildListAbilityItemRowView();
        }

        /// <summary>
        /// Create the list view based on the asset data
        /// </summary>
        private void GenerateListView(List<AbilityItem> abilityItems)
        {
            //Import the ListView Item Template
            var itemRowTemplate = AbilityToolUtils.LoadAbilityItemRowEditor();

            var itemTab = this.rootVisualElement.Q<VisualElement>("ItemsTab");
            //Create the listview and set various properties
            this.abilityItemListView             = itemTab.Q<ListView>("ListAbilityItemView");
            this.abilityItemListView.itemsSource = abilityItems;

            //Defining what each item will visually look like. In this case, the makeItem function is creating a clone of the ItemRowTemplate.
            this.abilityItemListView.makeItem = () => itemRowTemplate.CloneTree();
            //Define the binding of each individual Item that is created.
            this.abilityItemListView.bindItem = async (e, i) =>
            {
                if (abilityItems[i] == null) return;
                e.Q<Label>("Name").text                          = abilityItems[i].name;
                e.Q<VisualElement>("Icon").style.backgroundImage = (await abilityItems[i].Icon.LoadLocalSprite())?.texture;
            };
            this.abilityItemListView.selectionType = SelectionType.Single;

            this.abilityItemListView.selectionChanged += this.ListView_onSelectionChange;
            if (this.abilityInventory.Count > 0)
                this.abilityItemListView.SetSelection(this.lastSelectionItem);
        }

        #region Handle Event

        /// <summary>
        /// Add a new Item asset to the Asset/Data folder
        /// </summary>
        private void AddItem_OnClick()
        {
            //Create an instance of the scriptable object
            var newItem = CreateInstance<AbilityItem>();
            newItem.Id   = $"NewAbility{newItem.GetInstanceID()}";
            newItem.Name = $"New Item";

            //Create the asset 
            AssetDatabase.CreateAsset(newItem, $"{AbilityToolUtils.AbilityItemSOFolderPath}{newItem.Id}.asset");

            //Add it to the item list
            this.abilityInventory.Add(newItem);

            //Refresh the ListView so everything is redrawn again
            this.RebuildListAbilityItemRowView();

            this.abilityItemListView.SetSelection(this.abilityInventory.Count - 1);
        }

        private void DeleteItem_OnClick()
        {
            //Get the path of the fie and delete it through AssetDatabase
            string path = AssetDatabase.GetAssetPath(this.activeItem);
            AssetDatabase.DeleteAsset(path);

            //Purge the reference from the list and refresh the ListView
            this.abilityInventory.Remove(this.activeItem);
            this.RebuildListAbilityItemRowView();

            //Nothing is selected, so hide the details section
            this.detailPanelView.style.visibility = Visibility.Hidden;
        }

        private void ListView_onSelectionChange(IEnumerable<object> selectedItems)
        {
            //Get the first item in the selectedItems list. 
            //There will only ever be one because SelectionType is set to Single
            this.activeItem = (AbilityItem)selectedItems.First();
            //highlight on folder
            // EditorGUIUtility.PingObject(this.activeItem);

            var index = this.abilityInventory.FindIndex(x => x == this.activeItem);
            this.lastSelectionItem = index == -1 ? 0 : index;

            this.itemInfoEditor.BindAbilityDetailPanel(this.activeItem);

            //Make sure the detail section is visible. This can turn off when you delete an item
            this.detailPanelView.style.visibility = Visibility.Visible;
        }

        private void RebuildListAbilityItemRowView() { this.abilityItemListView.Rebuild(); }

        #endregion


        #region Pull Push Data

        private void PullDataFromSpreadSheet()
        {
            this.abilityInventory.Clear();
            if (this.configGoogleSheet.IsLocalResource)
            {
                string text = File.ReadAllText($"./{this.configGoogleSheet.LocalSheetPath}");
                this.UpdateCsv(text);
            }
            else
            {
                SyncGoogleSheetData.GetCsvFromWorkSheet(this.configGoogleSheet.SpreadSheetID, this.configGoogleSheet.WorkSheetName, this.UpdateCsv);
            }
        }
        private async void UpdateCsv(string csv)
        {
            if (string.IsNullOrEmpty(csv))
            {
                Debug.LogError($"CSV is Null");
                return;
            }

            var abilityBlueprint = new AbilityBlueprint();
            await abilityBlueprint.DeserializeFromCsv(csv);

            foreach (var ab in abilityBlueprint)
            {
                this.abilityInventory.Add(this.CheckFileExistsAndCreateNew(ab.Value));
            }

            //Refresh the ListView so everything is redrawn again
            this.RebuildListAbilityItemRowView();
            this.itemInfoEditor.Refresh();
            AssetDatabase.Refresh();
        }

        private AbilityItem CheckFileExistsAndCreateNew(AbilityRecord record)
        {
            var fileName = $"{AbilityToolUtils.AbilityItemSOFolderPath}{record.Id}.asset";

            if (File.Exists(fileName))
            {
                Debug.LogError($"Delete {fileName}");
                File.Delete(fileName);
                File.Delete($"{fileName}.meta");
            }

            var item = CreateInstance<AbilityItem>().FromAbilityRecord(record);
            //Create the asset 
            AssetDatabase.CreateAsset(item, fileName);
            Debug.LogWarning($"Create {fileName}");

            return item;
        }

        private async void PushDataToSpreadSheet()
        {
            this.pushToSheetBtn.SetEnabled(false);
            var startTime        = DateTime.Now;
            var abilityBlueprint = new AbilityBlueprint();
            foreach (var ablityScripableObject in this.abilityInventory)
            {
                abilityBlueprint.Add(ablityScripableObject.Id, ablityScripableObject.ToAbilityRecord());
            }

            if (this.configGoogleSheet.IsLocalResource)
            {
                var contents = abilityBlueprint.SerializeToRawData().Select(item =>
                {
                    var combinedString = string.Join(",", item.Select(s =>
                    {
                        if (string.IsNullOrEmpty(s))
                            return "";
                        var result = s;
                        if (s.Contains("\""))
                            result = s.Replace("\"", "\"\"");
                        if (s.Contains(",") || result.Contains("\"\""))
                            result = $"\"{result}\"";

                        return result;
                    }));
                    return combinedString;
                });

                await File.WriteAllLinesAsync($"./{this.configGoogleSheet.LocalSheetPath}", contents);
                Debug.Log($"Save to {this.configGoogleSheet.LocalSheetPath} complete - time = {(DateTime.Now - startTime).TotalSeconds}");
                AssetDatabase.Refresh();
            }
            else
            {
                SyncGoogleSheetData.PushAllData(this.configGoogleSheet.SpreadSheetID, this.configGoogleSheet.UploadSheetName, abilityBlueprint.SerializeToRawData(),
                    () => { Debug.LogError($"Push Complete"); });
            }

            this.pushToSheetBtn.SetEnabled(true);
        }

        #endregion

        #region Sheet config

        private EditorWindow inspectorWindow;
        private void SheetConfig()
        {
            if (this.inspectorWindow != null)
            {
                this.inspectorWindow.Focus();
                return;
            }

            // Retrieve the existing Inspector tab, or create a new one if none is open
            this.inspectorWindow = GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow"));
            // Get the size of the currently window
            this.inspectorWindow = Instantiate(this.inspectorWindow);
            // Set min size, and focus the window
            this.inspectorWindow.minSize = new Vector2(300, 300);
            this.inspectorWindow.maxSize = this.minSize;
            this.inspectorWindow.Show();
            this.inspectorWindow.Focus();


            this.configGoogleSheet = (GoogleSheetConfig)AssetDatabase.LoadAssetAtPath(AbilityToolUtils.PathGoogleSheetConfig, typeof(ScriptableObject));
            Selection.activeObject = this.configGoogleSheet;
        }

        private void ApiConfig()
        {
            var win = GetWindow<GoogleSheetsToUnityEditorWindow>("Config Api");
            ServicePointManager.ServerCertificateValidationCallback = Validator;

            bool Validator(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors) { return true; }
            win.Init();
        }

        #endregion
    }
}