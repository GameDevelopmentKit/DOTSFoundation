namespace GASCore.Systems.AbilityMainFlow.Components
{
    using System;
    using GASCore.Blueprints;
    using Unity.Collections;
    using Unity.Entities;

    #region Ability status

    public struct RequestActivate : IComponentData, IEnableableComponent
    {
    }

    public struct GrantedActivation : IComponentData, IEnableableComponent
    {
    }

    public struct FinishedComponent : IComponentData, IEnableableComponent
    {
    }

    #endregion


    public struct AbilityId : IComponentData
    {
        public FixedString64Bytes Value;
    }

    public struct PassiveAbilityTag : IComponentData
    {
    }

    public struct ActiveAbilityTag : IComponentData
    {
    }

    public struct AbilityCost : IBufferElementData
    {
        public FixedString64Bytes Name;
        public float              Value;
    }

    public struct CastRangeComponent : IComponentData
    {
        public float Value;
        public float ValueSqr => this.Value * this.Value;
    }

    public struct Cooldown : IComponentData
    {
        public float Value;
    }

    //Use for detect targetable objects of an ability
    public struct TargetTypeElement : IBufferElementData
    {
        public                          TargetType Value;
        public static implicit operator TargetType(TargetTypeElement targetType) => targetType.Value;
        public static implicit operator TargetTypeElement(TargetType targetType) => new() { Value = targetType };
    }

    public struct AbilityTimelinePrefabComponent : IComponentData
    {
        public Entity Value;
    }

    public struct AbilityTimelineInitialElement : IBufferElementData
    {
        public Entity Prefab;
    }

    public struct AbilityEffectElement : IBufferElementData
    {
        public Entity EffectPrefab;
    }

    public struct AbilityEffectPoolComponent : IComponentData
    {
        public BlobAssetReference<NativeHashMap<FixedString64Bytes, Entity>> BlobValue;
    }

    [Serializable]
    public struct AoE : IComponentData
    {
        public AoEType AoEType;
        public int     AoERange;
        public int     AoEWidth;
    }

    public struct MaxLevelTag : IComponentData
    {
    }
}