namespace GASCore.Systems.StatSystems.Components
{
    using System.Collections.Generic;
    using System.Linq;
    using DOTSCore.Extension;
    using GameFoundation.Scripts.Utilities.Extension;
    using GASCore.Interfaces;
    using GASCore.Services;
    using GASCore.Systems.AbilityMainFlow.Factories;
    using GASCore.Systems.LogicEffectSystems.Components;
    using GASCore.Systems.VisualEffectSystems.Components;
    using Sirenix.OdinInspector;
    using Unity.Collections;
    using Unity.Entities;
    using Zenject;

    #region Stat Modifier Components

    public struct ModifierAggregatorData : IBufferElementData
    {
        public FixedString64Bytes TargetStat;

        public float Add;
        public float Multiply;
        public float Divide;
        public float Override;

        public ModifierAggregatorData(FixedString64Bytes targetStat)
        {
            this.TargetStat = targetStat;
            this.Add        = 0;
            this.Multiply   = 1;
            this.Divide     = 1;
            this.Override   = -1;
        }

        public ModifierAggregatorData(FixedString64Bytes targetStat, float addValue)
        {
            this.TargetStat = targetStat;
            this.Add        = addValue;
            this.Multiply   = 1;
            this.Divide     = 1;
            this.Override   = -1;
        }

        public static ModifierAggregatorData GetDefault()
        {
            return new ModifierAggregatorData()
            {
                Add      = 0,
                Multiply = 1,
                Divide   = 1,
                Override = -1,
            };
        }
    }

    public struct StatModifierEntityElement : IBufferElementData
    {
        public                          Entity Value;
        public static implicit operator Entity(StatModifierEntityElement element) => element.Value;
        public static implicit operator StatModifierEntityElement(Entity element) => new() { Value = element };
    }

    public class StatModifierEntityAuthoring : IAbilityActionComponentConverter
    {
        [Inject] private AbilityActionEntityPrefabFactory actionEntityPrefabFactory;

        public List<EntityConverter.EntityData<IStatModifierComponentConverter>> StatModifiersData;

        public virtual void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
        {
            ZenjectUtils.GetCurrentContainer()?.Inject(this);
            ecb.AddBuffer<ModifierAggregatorData>(index, entity);
            var statModifierBuffers = ecb.AddBuffer<StatModifierEntityElement>(index, entity);
            var temp = this.StatModifiersData.Select(data =>
            {
                var temp1 = data.components.Select(converter => (IComponentConverter)converter).ToList();

                return new EntityConverter.EntityData<IComponentConverter>() { components = temp1 };
            }).ToList();
            var listEntityActionPrefab =
                this.actionEntityPrefabFactory.CreateAbilityActionEntityPrefabsFromJson(ecb, index, temp);
            foreach (var statModifierPrefab in listEntityActionPrefab)
            {
                statModifierBuffers.Add(new StatModifierEntityElement() { Value = statModifierPrefab });
            }

            ecb.AddChildren(index, entity, listEntityActionPrefab);
        }
    }

    #endregion

    #region Stat modifier element data

    public struct StatModifierData : IComponentData
    {
        public FixedString64Bytes   TargetStat;
        public ModifierOperatorType ModifierOperator;

        public float ModifierMagnitude;

        public class _ : IStatModifierComponentConverter
        {
            [ValueDropdown("GetFieldValues", AppendNextDrawer = true)]
            public string Stat;

            public ModifierOperatorType ModifierOperator;

            public List<string> GetFieldValues() => AbilityHelper.GetListStatName();

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

            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity) { ecb.AddComponent(index, entity, new ScalableFloatMagnitudeCalculation() { Value = this.Value }); }
        }
    }

    public struct RandomIntInRangeMagnitudeCalculation : IMagnitudeCalculation
    {
        public int Min;
        public int Max;

        public class _ : IStatModifierComponentConverter
        {
            public SimpleIntRange Range;

            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
            {
                ecb.AddComponent(index, entity, new RandomIntInRangeMagnitudeCalculation()
                {
                    Min = this.Range.min,
                    Max = this.Range.max,
                });
            }
        }
    }

    //Capture source attribute
    public struct StatBasedMagnitudeCalculation : IMagnitudeCalculation
    {
        public float              Coefficient;
        public FixedString64Bytes SourceStat;
        public SourceType         SourceType;

        public class _ : IStatModifierComponentConverter
        {
            public float Coefficient = 1.0f;

            [ValueDropdown("GetFieldValues", AppendNextDrawer = true)]
            public string SourceStat;

            public SourceType SourceType;

            public List<string> GetFieldValues() => AbilityHelper.GetListStatName();
            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
            {
                ecb.AddComponent(index, entity, new StatBasedMagnitudeCalculation()
                {
                    Coefficient = this.Coefficient,
                    SourceStat  = this.SourceStat,
                    SourceType  = this.SourceType,
                });
            }
        }
    }

    #endregion

    public enum ModifierOperatorType : uint
    {
        Add      = 0, // use negative number for sub
        Multiply = 1,
        Divide   = 2,
        Override = 3
    }
}