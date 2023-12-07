namespace GASCore.Systems.AbilityMainFlow.Components
{
    using System;
    using GASCore.Blueprints;
    using GASCore.Interfaces;
    using Unity.Collections;
    using Unity.Entities;

    #region Ability status

    public struct RequestActivate : IComponentData, IEnableableComponent { }

    public struct RequestEnd : IComponentData, IEnableableComponent { }

    public struct GrantedActivation : IComponentData, IEnableableComponent { }


    public struct ActivatedTag : IComponentData, IEnableableComponent { }

    public struct FinishedComponent : IComponentData, IEnableableComponent { }

    public struct ActiveOneTimeTag : IComponentData, IAbilityActivateConditionConverter
    {
        public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity) { ecb.AddComponent<ActiveOneTimeTag>(index, entity); }
    }

    #endregion


    public struct AbilityId : IComponentData
    {
        public FixedString64Bytes Value;
    }

    public struct AbilityLevel : IComponentData
    {
        public int Value;
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
        public float ValueSqr => this.Value * this.Value;
    }

    public struct Cooldown : IComponentData
    {
        public float Value;
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

    public struct MaxLevelTag : IComponentData { }
}