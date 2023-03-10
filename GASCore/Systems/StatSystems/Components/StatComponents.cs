using System;
using System.Collections.Generic;
using GASCore.Interfaces;
using GASCore.Systems.TimelineSystems.Components;
using Unity.Collections;
using Unity.Entities;

namespace GASCore.Systems.StatSystems.Components
{
    public struct StatDataElement : IBufferElementData
    {
        public FixedString64Bytes StatName;
        public float              OriginValue;
        public float              BaseValue;
        public float              AddedValue;

        public float CurrentValue => this.BaseValue + this.AddedValue;
    }


    public class StatDataAuthoring : IAbilityActionComponentConverter
    {
        [Serializable]
        public class StatElement
        {
            public string StatName;
            public float  BaseValue;
        }

        public List<StatElement> Value;

        public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
        {
            var statBuffer  = ecb.AddBuffer<StatDataElement>(index, entity);

            for (var i = 0; i < this.Value.Count; i++)
            {
                var stat = this.Value[i];
                statBuffer.Add(new StatDataElement()
                {
                    StatName    = stat.StatName,
                    OriginValue = stat.BaseValue,
                    BaseValue   = stat.BaseValue,
                    AddedValue  = 0
                });
            }
        }
    }

    public struct StatNameToIndex : IComponentData
    {
        public BlobAssetReference<NativeHashMap<FixedString64Bytes, int>> BlobValue;
    }

    public readonly partial struct StatAspect : IAspect
    {
        private readonly Entity                         sourceEntity;
        private readonly DynamicBuffer<StatDataElement> statDataBuffer;
        private readonly RefRO<StatNameToIndex>         statNameToIndex;

        public bool SetBaseValue(FixedString64Bytes statName, float newValue)
        {
            if (!this.statNameToIndex.ValueRO.BlobValue.Value.TryGetValue(statName, out var statIndex)) return false;
            var statData = this.statDataBuffer[statIndex];
            statData.BaseValue = newValue;
            this.statDataBuffer.RemoveAt(statIndex);
            this.statDataBuffer.Insert(statIndex, statData);
            return true;
        }

        public bool SetAddedValue(FixedString64Bytes statName, float newValue)
        {
            if (!this.statNameToIndex.ValueRO.BlobValue.Value.TryGetValue(statName, out var statIndex)) return false;
            var statData = this.statDataBuffer[statIndex];
            statData.AddedValue = newValue;
            this.statDataBuffer.RemoveAt(statIndex);
            this.statDataBuffer.Insert(statIndex, statData);
            return true;
        }

        public int GetStatCount() { return this.statDataBuffer.Length; }

        public float GetBaseValue(FixedString64Bytes statName)
        {
            if (this.statNameToIndex.ValueRO.BlobValue.Value.TryGetValue(statName, out var statIndex))
                return this.statDataBuffer[statIndex].BaseValue;

            return 0;
        }

        public float GetCurrentValue(FixedString64Bytes statName)
        {
            if (this.statNameToIndex.ValueRO.BlobValue.Value.TryGetValue(statName, out var statIndex))
                return this.statDataBuffer[statIndex].CurrentValue;

            return 0;
        }

        public int GetStatIndex(FixedString64Bytes statName)
        {
            if (this.statNameToIndex.ValueRO.BlobValue.Value.TryGetValue(statName, out var statIndex))
            {
                return statIndex;
            }

            return -1;
        }

        public void NotifyStatChange(EntityCommandBuffer.ParallelWriter ecb, int entityInQueryIndex, FixedString64Bytes statName)
        {
            if (!this.statNameToIndex.ValueRO.BlobValue.Value.TryGetValue(statName, out var statIndex)) return;
            ecb.SetComponentEnabled<OnStatChange>(entityInQueryIndex, this.sourceEntity, true);
            ecb.AppendToBuffer(entityInQueryIndex, this.sourceEntity, new OnStatChange()
            {
                ChangedStat = this.statDataBuffer[statIndex]
            });
        }

        public bool HasStat(FixedString64Bytes statName) { return this.statNameToIndex.ValueRO.BlobValue.Value.ContainsKey(statName); }

        public float CalculateStatValue(ModifierAggregatorData aggregator)
        {
            //todo handle case override = 0
            return aggregator.Override != 0 ? aggregator.Override : (GetBaseValue(aggregator.TargetStat) + aggregator.Add) * aggregator.Multiply / aggregator.Divide;
        }
    }
}