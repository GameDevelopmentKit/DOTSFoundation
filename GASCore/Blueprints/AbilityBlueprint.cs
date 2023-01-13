namespace GASCore.Blueprints
{
    using System;
    using System.Collections.Generic;
    using BlueprintFlow.BlueprintReader;
    using GASCore.Systems.AbilityMainFlow.Components;

    /// <summary>Define a ability data, Name, Description, Icon, Effects....</summary>
    [BlueprintReader("Ability")]
    public class AbilityBlueprint : GenericBlueprintReaderByRow<string, AbilityRecord>
    {
        public List<TargetType> AffectedTarget(BlueprintByRow<string, AbilityEffect> abilityEffectPool)
        {
            List<TargetType> listAffectedTarget = new List<TargetType>();
            foreach (var abilityEffect in abilityEffectPool)
            {
                listAffectedTarget.AddRange(abilityEffect.Value.AffectedTarget);
            }

            return listAffectedTarget;
        }
    }

    [Serializable]
    [CsvHeaderKey("Id")]
    public struct AbilityRecord
    {
        public string            Id;
        public string            Name;
        public string            Description;
        public AbilityType       Type;
        public string            Icon;
        public List<TargetType>  Target;
        public AbilityEffectType EffectCategory;

        public BlueprintByRow<AbilityLevelRecord> LevelRecords;

        public AbilityLevelRecord GetLevelRecord(int level) => this.LevelRecords[level - 1];
    }

    [Serializable]
    [CsvHeaderKey("LevelIndex")]
    public struct AbilityLevelRecord
    {
        public int                       LevelIndex;
        public float                     Cooldown;
        public Dictionary<string, float> Cost;
        public float                     CastRange;

        public string                        AbilityActivateCondition;
        public string                        AbilityTimeline;
        public BlueprintByRow<AbilityEffect> AbilityEffectPool;
    }

    [Serializable]
    [CsvHeaderKey("EffectId")]
    public struct AbilityEffect
    {
        public string           EffectId;
        public List<TargetType> AffectedTarget;
        public string           EffectData;
    }

    public enum AbilityType
    {
        Active,
        Passive
    }

    public enum AoEType
    {
        Single,
        Straight,
        Round,
        Cone,
        Star,
        HalfRound
    }

    public enum AbilityEffectType
    {
        Damage,
        Movable,
        Heal
    }

    public enum TargetType
    {
        None,
        Opponent,
        Caster,
        Ally,
    }
}