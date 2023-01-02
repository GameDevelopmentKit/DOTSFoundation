namespace GASCore.Systems.StatSystems.Components
{
    using System.Collections.Generic;
    using System.Linq;
    using DOTSCore.Extension;
    using GameFoundation.Scripts.Utilities.Extension;
    using GASCore.Interfaces;
    using GASCore.Services;
    using GASCore.Systems.AbilityMainFlow.Factories;
    using Unity.Collections;
    using Unity.Entities;
    using Zenject;

    public struct ModifierDataAggregator
    {
        public FixedString64Bytes TargetStat;

        public float Add;
        public float Multiply;
        public float Division;
        public float Override;
    }

    public struct StatModifierEntityElement : IBufferElementData
    {
        public Entity Value;
    }

    public class StatModifierEntityBufferAuthoring : IAbilityActionComponentConverter
    {
        [Inject] private AbilityActionEntityPrefabFactory actionEntityPrefabFactory;
        
        public List<EntityConverter.EntityData<IStatModifierComponentConverter>> StatModifiersData;

        public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
        {
            ZenjectUtils.GetCurrentContainer()?.Inject(this);
            var statModifierBuffers = ecb.AddBuffer<StatModifierEntityElement>(index, entity);
            var temp = this.StatModifiersData.Select(data =>
            {
                var temp1 = data.components.Select(converter => (IComponentConverter)converter).ToList();

                return new EntityConverter.EntityData<IComponentConverter>() { components = temp1 };
            }).ToList();
            var listEntityActionPrefab = this.actionEntityPrefabFactory.CreateAbilityActionEntityPrefabsFromJson(ecb, index, temp);
            foreach (var statModifierPrefab in listEntityActionPrefab)
            {
                statModifierBuffers.Add(new StatModifierEntityElement() { Value = statModifierPrefab });
            }

            ecb.AddChildren(index, entity, listEntityActionPrefab);
        }
    }
    
    #region stat modifier data

    public struct StatModifierData : IComponentData
    {
        public FixedString64Bytes   TargetStat;
        public ModifierOperatorType ModifierOperator;

        public float ModifierMagnitude;

        public class _ : IStatModifierComponentConverter
        {
            public string               Stat;
            public ModifierOperatorType ModifierOperator;
            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
            {
                ecb.AddComponent(index, entity, new StatModifierData()
                {
                    TargetStat       = this.Stat,
                    ModifierOperator = this.ModifierOperator
                });
            }
        }
    }

    public interface IMagnitudeCalculation : IComponentData { }

    public struct ScalableFloatMagnitudeCalculation : IMagnitudeCalculation
    {
        public float Value;

        public class _ : IStatModifierComponentConverter
        {
            public float Value;
            public void  Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity) { ecb.AddComponent(index, entity, new ScalableFloatMagnitudeCalculation() { Value = this.Value }); }
        }
    }

    //Capture source attribute
    public struct AttributeBasedMagnitudeCalculation : IMagnitudeCalculation
    {
        public float              Coefficient;
        public FixedString64Bytes SourceAttribute;
        public SourceType         SourceType;
        public bool               SnapShot;

        public class _ : IStatModifierComponentConverter
        {
            public float      Coefficient;
            public string     SourceAttribute;
            public SourceType SourceType;
            public bool       SnapShot;
            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
            {
                ecb.AddComponent(index, entity, new AttributeBasedMagnitudeCalculation()
                {
                    Coefficient     = this.Coefficient,
                    SourceAttribute = this.SourceAttribute,
                    SourceType      = this.SourceType,
                    SnapShot        = this.SnapShot,
                });
            }
        }
    }

    #endregion

    public enum SourceType
    {
        Source,
        Target
    }

    public enum ModifierOperatorType : uint
    {
        Add      = 0, // use negative number for sub
        Multiply = 1,
        Division = 2,
        Override = 3
    }
}