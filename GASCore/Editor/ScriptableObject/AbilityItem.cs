namespace GASCore.Editor.ScriptableObject
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BlueprintFlow.BlueprintReader;
    using GASCore.Blueprints;
    using GASCore.Interfaces;
    using GASCore.Services;
    using GASCore.Systems.AbilityMainFlow.Components;
    using Sirenix.OdinInspector;
    using UnityEngine;

    [CreateAssetMenu(fileName = "AbilityItem", menuName = "GDK/Ability/AbilityItem")]
    public class AbilityItem : SerializedScriptableObject
    {
        public string            Id;
        public string            Name;
        public string            Description;
        public AbilityType       Type;
        public string            Icon;
        public List<TargetType>  Target;
        public AbilityEffectType EffectCategory;

        [ListDrawerSettings(ShowPaging = true, ListElementLabelName = "LevelTitle", OnBeginListElementGUI = "BeginListElement", NumberOfItemsPerPage = 5)]
        public List<AbilityLevelEditorData> AbilityLevelEditorRecords;

        #region Editor Function

        public void BeginListElement(int index)
        {
            this.AbilityLevelEditorRecords[index].LevelTitle = $"Level {index + 1} Data";
            this.AbilityLevelEditorRecords[index].LevelIndex = index;
        }

        #endregion

        public AbilityRecord ToAbilityRecord()
        {
            var levelRecords = new BlueprintByRow<AbilityLevelRecord>();
            foreach (var levelEditorRecord in this.AbilityLevelEditorRecords)
            {
                var abilityEffectPool = new BlueprintByRow<AbilityEffect>();
                abilityEffectPool.AddRange(levelEditorRecord.listAbilityEffects.Select(effect => new AbilityEffect()
                    { EffectId = effect.EffectId, AffectedTarget = effect.AffectedTarget, EffectData = effect.EffectData.ConvertEntitiesDataToJson(), }));

                levelRecords.Add(new AbilityLevelRecord()
                {
                    LevelIndex = levelEditorRecord.LevelIndex,
                    Cooldown   = levelEditorRecord.Cooldown,
                    Cost       = levelEditorRecord.Cost,
                    CastRange  = levelEditorRecord.CastRange,

                    AbilityActivateCondition = levelEditorRecord.abilityActivateConditionComponents.ConvertComponentsDataToJson(),
                    AbilityTimeline          = levelEditorRecord.timelineComponents.ConvertEntitiesDataToJson(),
                    AbilityEffectPool        = abilityEffectPool
                });
            }

            return new AbilityRecord()
            {
                Id             = this.Id,
                Name           = this.Name,
                Description    = this.Description,
                Type           = this.Type,
                Icon           = this.Icon,
                Target         = this.Target,
                EffectCategory = this.EffectCategory,
                LevelRecords   = levelRecords
            };
        }
        public AbilityItem FromAbilityRecord(AbilityRecord abilityRecord)
        {
            this.Id                        = abilityRecord.Id;
            this.Name                      = abilityRecord.Name;
            this.Description               = abilityRecord.Description;
            this.Type                      = abilityRecord.Type;
            this.Icon                      = abilityRecord.Icon;
            this.Target                    = abilityRecord.Target;
            this.EffectCategory            = abilityRecord.EffectCategory;
            this.AbilityLevelEditorRecords = new List<AbilityLevelEditorData>();
            foreach (var levelRecord in abilityRecord.LevelRecords)
            {
                var abilityEffectEditors = new List<AbilityEffectEditor>();
                abilityEffectEditors.AddRange(levelRecord.AbilityEffectPool.Select(effect => new AbilityEffectEditor()
                    { EffectId = effect.EffectId, AffectedTarget = effect.AffectedTarget, EffectData = effect.EffectData.ConvertJsonToEntitiesData<IAbilityActionComponentConverter>(), }));

                var abilityLevelEditorData = new AbilityLevelEditorData
                {
                    LevelIndex                         = levelRecord.LevelIndex,
                    Cooldown                           = levelRecord.Cooldown,
                    Cost                               = levelRecord.Cost,
                    CastRange                          = levelRecord.CastRange,
                    abilityActivateConditionComponents = levelRecord.AbilityActivateCondition.ConvertJsonToComponentsData<IAbilityActivateConditionConverter>(),
                    timelineComponents                 = levelRecord.AbilityTimeline.ConvertJsonToEntitiesData<ITimelineActionComponentConverter>(),
                    listAbilityEffects                 = abilityEffectEditors
                };

                this.AbilityLevelEditorRecords.Add(abilityLevelEditorData);
            }

            return this;
        }
    }


    [Serializable]
    public class AbilityLevelEditorData
    {
        internal                                   string                    LevelTitle = " Level Data";
        [Header("General Data")] [ReadOnly] public int                       LevelIndex;
        public                                     float                     Cooldown;
        public                                     Dictionary<string, float> Cost;
        public                                     float                     CastRange;

        [SerializeReference] public List<IAbilityActivateConditionConverter> abilityActivateConditionComponents = new();


        [HorizontalGroup(GroupID = "ActivateComponentGroup", Title = "Activated Components Group", Width = 0.3f)] [ListDrawerSettings(ShowIndexLabels = true)]
        public List<EntityConverter.EntityData<ITimelineActionComponentConverter>> timelineComponents = new();

        [HorizontalGroup(GroupID = "ActivateComponentGroup")]
        public List<AbilityEffectEditor> listAbilityEffects = new();

#if UNITY_EDITOR
        [FoldoutGroup("Raw Data", false, GroupID = "RawData")]
        public string AbilityActivateConditionRawData;

        [FoldoutGroup("Raw Data", false, GroupID = "RawData")]
        public string AbilityTimelineRawData;

        [FoldoutGroup("Raw Data", false, GroupID = "RawData")]
        public List<AbilityEffect> AbilityEffectPoolRawData;

        [FoldoutGroup("Raw Data", false, GroupID = "RawData")]
        [Button]
        public void ConvertToListEntities()
        {
            this.abilityActivateConditionComponents.Clear();
            this.timelineComponents.Clear();
            this.listAbilityEffects.Clear();

            this.abilityActivateConditionComponents = this.AbilityActivateConditionRawData.ConvertJsonToComponentsData<IAbilityActivateConditionConverter>();
            this.timelineComponents                 = this.AbilityTimelineRawData.ConvertJsonToEntitiesData<ITimelineActionComponentConverter>();

            foreach (var effect in this.AbilityEffectPoolRawData)
            {
                this.listAbilityEffects.Add(new AbilityEffectEditor()
                {
                    EffectId       = effect.EffectId,
                    AffectedTarget = effect.AffectedTarget,
                    EffectData     = effect.EffectData.ConvertJsonToEntitiesData<IAbilityActionComponentConverter>(),
                });
            }
        }

        [FoldoutGroup("Raw Data", false, GroupID = "RawData")]
        [Button]
        public void ConvertToJson()
        {
            this.AbilityActivateConditionRawData = this.abilityActivateConditionComponents.ConvertComponentsDataToJson();
            this.AbilityTimelineRawData          = this.timelineComponents.ConvertEntitiesDataToJson();

            this.AbilityEffectPoolRawData.Clear();
            foreach (var effect in this.listAbilityEffects)
            {
                this.AbilityEffectPoolRawData.Add(new AbilityEffect()
                {
                    EffectId       = effect.EffectId,
                    AffectedTarget = effect.AffectedTarget,
                    EffectData     = effect.EffectData.ConvertEntitiesDataToJson(),
                });
            }
        }
#endif
    }

    [Serializable]
    public class AbilityEffectEditor
    {
        public string                                                             EffectId;
        public List<TargetType>                                                   AffectedTarget;
        public List<EntityConverter.EntityData<IAbilityActionComponentConverter>> EffectData;
    }
}