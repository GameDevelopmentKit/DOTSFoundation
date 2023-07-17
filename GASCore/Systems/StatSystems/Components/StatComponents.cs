using System;
using System.Collections.Generic;
using DOTSCore.Extension;
using GASCore.Interfaces;
using Unity.Collections;
using Unity.Entities;

namespace GASCore.Systems.StatSystems.Components
{
    using GASCore.Services;
    using Sirenix.OdinInspector;

    public struct StatDataElement : IBufferElementData
    {
        public FixedString64Bytes StatName;
        public float              OriginValue;
        public float              BaseValue;
        public float              AddedValue;
        public float              CurrentValue => this.BaseValue + this.AddedValue;

        public bool IsDirty;

        public StatDataElement(FixedString64Bytes statName, float originValue)
        {
            this.StatName    = statName;
            this.OriginValue = originValue;
            this.BaseValue   = originValue;
            this.AddedValue  = 0;
            this.IsDirty     = false;
        }
    }

    public class StatDataAuthoring : IAbilityActionComponentConverter
    {
        [Serializable]
        public class StatElement
        {
            [ValueDropdown("GetFieldValues")]
            public string StatName;
            public float  BaseValue;
            
            public List<string> GetFieldValues() => AbilityHelper.GetListStatName();
        }

        public List<StatElement> Value;

        public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
        {
            var statBuffer = ecb.AddBuffer<StatDataElement>(index, entity);
            foreach (var stat in this.Value)
            {
                statBuffer.Add(new StatDataElement(stat.StatName, stat.BaseValue));
            }
        }
    }

    public struct StatNameToIndex : IComponentData
    {
        private BlobAssetReference<NativeHashMap<FixedString64Bytes, int>> blobValue;

        public NativeHashMap<FixedString64Bytes, int> Value { get => this.blobValue.Value; set => this.blobValue = value.CreateReference(); }
    }

    public readonly partial struct StatAspect : IAspect
    {
        private readonly DynamicBuffer<StatDataElement> statDataBuffer;
        private readonly RefRO<StatNameToIndex>         statNameToIndex;

        public bool SetBaseValue(FixedString64Bytes statName, float newValue)
        {
            if (!this.statNameToIndex.ValueRO.Value.TryGetValue(statName, out var statIndex)) return false;
            var statDataBufferTemp = this.statDataBuffer;
            var statDataElement    = statDataBufferTemp[statIndex];
            statDataElement.BaseValue     = newValue;
            statDataElement.IsDirty       = true;
            statDataBufferTemp[statIndex] = statDataElement;
            return true;
        }

        public bool SetAddedValue(FixedString64Bytes statName, float newValue)
        {
            if (!this.statNameToIndex.ValueRO.Value.TryGetValue(statName, out var statIndex)) return false;
            var statDataBufferTemp = this.statDataBuffer;
            var statDataElement    = statDataBufferTemp[statIndex];
            statDataElement.AddedValue    = newValue;
            statDataElement.IsDirty       = true;
            statDataBufferTemp[statIndex] = statDataElement;
            return true;
        }

        public void ResetAllAddedValue()
        {
            var statDataBufferTemp = this.statDataBuffer;
            for (var index = 0; index < statDataBufferTemp.Length; index++)
            {
                var statDataElement = statDataBufferTemp[index];
                if (statDataElement.AddedValue == 0) continue;
                statDataElement.AddedValue = 0;
                statDataElement.IsDirty    = true;
                statDataBufferTemp[index]  = statDataElement;
            }
        }

        public int GetStatCount() { return this.statDataBuffer.Length; }

        public float GetBaseValue(FixedString64Bytes statName)
        {
            if (this.statNameToIndex.ValueRO.Value.TryGetValue(statName, out var statIndex))
                return this.statDataBuffer[statIndex].BaseValue;

            return 0;
        }

        public float GetCurrentValue(FixedString64Bytes statName)
        {
            if (this.statNameToIndex.ValueRO.Value.TryGetValue(statName, out var statIndex))
                return this.statDataBuffer[statIndex].CurrentValue;

            return 0;
        }

        public int GetStatIndex(FixedString64Bytes statName)
        {
            if (this.statNameToIndex.ValueRO.Value.TryGetValue(statName, out var statIndex))
            {
                return statIndex;
            }

            return -1;
        }

        public StatDataElement? GetStatData(FixedString64Bytes statName)
        {
            return this.statNameToIndex.ValueRO.Value.TryGetValue(statName, out var statIndex)
                ? this.statDataBuffer[statIndex]
                : null;
        }

        public bool HasStat(FixedString64Bytes statName) { return this.statNameToIndex.ValueRO.Value.ContainsKey(statName); }

        public float CalculateStatValue(ModifierAggregatorData aggregator)
        {
            //todo handle case override = 0
            return aggregator.Override != 0
                ? aggregator.Override
                : (GetBaseValue(aggregator.TargetStat) + aggregator.Add) * aggregator.Multiply / aggregator.Divide;
        }
    }
}