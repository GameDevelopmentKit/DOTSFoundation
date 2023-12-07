namespace GASCore.Systems.StatSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.StatSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityLogicEffectGroup))]
    [UpdateAfter(typeof(CalculateStatModifierMagnitudeSystem))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct AggregateStatModifierSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new AggregateStatModifierSystemJob()
            {
                StatModifierDataLookup = SystemAPI.GetComponentLookup<StatModifierData>(true),
            }.ScheduleParallel();
        }
    }


    /// <summary>
    /// Aggregate stat modifier for each effect entity
    /// </summary>
    [BurstCompile]
    [WithChangeFilter(typeof(StatModifierEntityElement))]
    public partial struct AggregateStatModifierSystemJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<StatModifierData>  StatModifierDataLookup;
        void Execute(in DynamicBuffer<StatModifierEntityElement> statModifierEntityElementBuffers, ref DynamicBuffer<ModifierAggregatorData> modifierAggregatorDataBuffer)
        {
            //aggregate all modifier
            var statNameToModifierAggregators = new NativeHashMap<FixedString64Bytes, ModifierAggregatorData>(statModifierEntityElementBuffers.Length, Allocator.Temp);
            foreach (var statModifierElementEntity in statModifierEntityElementBuffers)
            {
                var statModifierData = this.StatModifierDataLookup[statModifierElementEntity.Value];

                if (!statNameToModifierAggregators.TryGetValue(statModifierData.TargetStat, out var dataAggregator))
                {
                    dataAggregator = new ModifierAggregatorData(statModifierData.TargetStat);
                }

                switch (statModifierData.ModifierOperator)
                {
                    case ModifierOperatorType.Add:
                        dataAggregator.Add += statModifierData.ModifierMagnitude;
                        break;
                    case ModifierOperatorType.Multiply:
                        dataAggregator.Multiply *= statModifierData.ModifierMagnitude;
                        break;
                    case ModifierOperatorType.Divide:
                        dataAggregator.Divide *= statModifierData.ModifierMagnitude;
                        break;
                    case ModifierOperatorType.Override:
                        dataAggregator.Override = statModifierData.ModifierMagnitude;
                        break;
                }

                statNameToModifierAggregators[statModifierData.TargetStat] = dataAggregator;
            }

            // cache aggregated modifier to buffer
            modifierAggregatorDataBuffer.Clear();
            foreach (var modifierAggregatorData in statNameToModifierAggregators)
            {
                modifierAggregatorDataBuffer.Add(modifierAggregatorData.Value);
            }
        }
    }
}