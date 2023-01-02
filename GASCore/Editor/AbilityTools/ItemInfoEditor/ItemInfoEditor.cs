namespace GASCore.Editor.AbilityTools.ItemInfoEditor
{
    using System;
    using System.Threading.Tasks;
    using GASCore.Editor.AbilityTools.MainEditor;
    using GASCore.Editor.ScriptableObject;
    using Sirenix.OdinInspector.Editor;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;
    using Object = UnityEngine.Object;

    public class ItemInfoEditor : VisualElement
    {
        private VisualElement  abilityGeneralInfoView;
        private IMGUIContainer abilityLevelEditorView;

        private AbilityItem currentSelectedAbility;

        public Action OnUpdateAbilityData;

        public ItemInfoEditor()
        {
            var template = AbilityToolUtils.LoadAbilityItemInfoEditor();
            this.Add(template.CloneTree());

            this.abilityGeneralInfoView = this.Q("General");
            this.abilityLevelEditorView = new IMGUIContainer();
            this.Q("LevelArea").Add(this.abilityLevelEditorView);
            this.abilityGeneralInfoView.Q<ObjectField>("IconPicker").RegisterValueChangedCallback(this.OnChangeAbilityIcon);
        }

        public void BindAbilityDetailPanel(AbilityItem selectedAbility)
        {
            this.currentSelectedAbility = selectedAbility;
            this.BindAbilityBaseInfoView(selectedAbility);
        }

        private async void BindAbilityBaseInfoView(AbilityItem selectedAbility)
        {
            //Create a new SerializedObject and bind the Details VE to it. 
            //This cascades the binding to the children
            var so = new SerializedObject(selectedAbility);
            this.abilityGeneralInfoView.Clear();
            this.abilityGeneralInfoView.Add(so.CreateUIElementInspector("AbilityLevelEditorRecords"));
            this.LoadLevelEditor(so);

            //Set the icon if it exists
            if (!string.IsNullOrEmpty(selectedAbility.Icon))
            {
                var iconSkillTexture = (await selectedAbility.Icon.LoadLocalSprite()).texture;
                this.Q<VisualElement>("AbilityIconImage").style.backgroundImage = iconSkillTexture;
                this.Q<ObjectField>("IconPicker").SetValueWithoutNotify(iconSkillTexture);
            }
            else
            {
                this.Q<VisualElement>("AbilityIconImage").style.backgroundImage = null;
                this.Q<ObjectField>("IconPicker").SetValueWithoutNotify(null);
            }

            so.ApplyModifiedProperties();

            //Need to wait after the editor view was loaded, if not, querying "unity-input-Id" text field will be missing
            await Task.Delay(1000);
            this.abilityGeneralInfoView.Q<TextField>("unity-input-Id").RegisterCallback<FocusOutEvent>(this.OnChangeAbilityId);
        }

        public void Refresh()
        {
            this.abilityGeneralInfoView.Unbind();
            this.abilityLevelEditorView.onGUIHandler = null;
        }

        private void LoadLevelEditor(SerializedObject property)
        {
            var levelEditorPropertiesTree = PropertyTree.Create(property);

            var levelEditorProperty = levelEditorPropertiesTree.GetPropertyAtPath("AbilityLevelEditorRecords");
            this.abilityLevelEditorView.onGUIHandler = () => DoDrawIMGUIProperty(levelEditorProperty);

            void DoDrawIMGUIProperty(InspectorProperty inspectorProperty)
            {
                levelEditorPropertiesTree.BeginDraw(true);
                inspectorProperty.Draw();
                levelEditorPropertiesTree.EndDraw();
            }
        }


        private void OnChangeAbilityIcon(ChangeEvent<Object> evt)
        {
            var newSprite = evt.newValue as Sprite;
            if (newSprite != null)
            {
                this.currentSelectedAbility.Icon                                           = newSprite.name;
                this.abilityGeneralInfoView.Q<VisualElement>("Icon").style.backgroundImage = newSprite.texture;
            }

            this.OnUpdateAbilityData?.Invoke();
        }

        private void OnChangeAbilityId(FocusOutEvent evt)
        {
            var textField = evt.target as TextField;
            AssetDatabase.RenameAsset($"{AbilityToolUtils.AbilityItemSOFolderPath}{currentSelectedAbility.name}.asset", textField.value);
            this.OnUpdateAbilityData?.Invoke();
        }
    }
}