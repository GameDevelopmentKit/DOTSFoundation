namespace TutorialSystem.Blueprint
{
    using System;
    using System.Collections.Generic;
    using Sirenix.OdinInspector;
    using TaskModule.Authoring;
    using UnityEngine;

    public class TutorialBlueprint : BaseSOBlueprint<TutorialBlueprint>
    {
        public bool EnableFTUE = true;
        [ListDrawerSettings(ShowPaging = true, ListElementLabelName = "Name", OnBeginListElementGUI = "BeginListElement", NumberOfItemsPerPage = 5)]
        public List<TutorialRecord> TutorialRecords = new();

        #region Editor Function

        public void BeginListElement(int index) { this.TutorialRecords[index].ID = index; }

        #endregion
    }

    [Serializable]
    public class TutorialRecord
    {
        [ReadOnly] public int    ID;
        public            string Name;
        public            string Description;

        [ListDrawerSettings(OnBeginListElementGUI = "BeginDrawListElement")]
        public List<TaskEntityData> Tasks;

        #if UNITY_EDITOR

        private void BeginDrawListElement(int index)
        {
            Sirenix.Utilities.Editor.SirenixEditorGUI.Title("Task Order: " + index, null, TextAlignment.Left, true);
        }
        #endif
    }


}