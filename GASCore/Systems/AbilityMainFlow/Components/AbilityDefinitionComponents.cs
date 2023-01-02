namespace GASCore.Systems.AbilityMainFlow.Components
{
    using System;
    using GASCore.Blueprints;
    using Unity.Collections;
    using Unity.Entities;

    #region Ability status

    public struct RequestActivate : IComponentData, IEnableableComponent { }

    public struct GrantedActivation : IComponentData, IEnableableComponent { }

    public struct FinishedComponent : IComponentData, IEnableableComponent { }

    #endregion


    public struct AbilityId : IComponentData
    {
        public FixedString64Bytes Value;
    }

    public struct PassiveAbilityTag : IComponentData { }

    public struct ActiveAbilityTag : IComponentData { }

    public struct AbilityCost : IBufferElementData
    {
        public FixedString64Bytes Name;
        public float              Value;
    }

    public struct CastRangeComponent : IComponentData
    {
        public float Value;
    }

    public struct Cooldown : IComponentData
    {
        public float Value;
    }

    public struct Duration : IComponentData, IEnableableComponent
    {
        public float Value;
    }

    public struct TargetTypeElement : IBufferElementData
    {
        public TargetType Value;
    }

    public struct AbilityTimelinePrefabComponent : IComponentData
    {
        public Entity Value;
    }

    public struct AbilityEffectPoolComponent : IBufferElementData
    {
        public Entity EffectPrefab;
    }

    [Serializable]
    public struct AoE : IComponentData
    {
        public AoEType AoEType;
        public int     AoERange;
        public int     AoEWidth;
    }
    
    public struct MaxLevelTag: IComponentData{}
}