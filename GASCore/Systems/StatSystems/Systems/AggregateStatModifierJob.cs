//Old AggregateStatModifierJob

// namespace GASCore.Systems.StatSystems.Systems
// {
//     using System;
//     using GASCore.Systems.AbilityMainFlow.Components;
//     using GASCore.Systems.StatSystems.Components;
//     using Unity.Burst;
//     using Unity.Collections;
//     using Unity.Entities;
//     using Unity.Jobs;
//     using UnityEngine;
//
//     [BurstCompile]
//     [WithChangeFilter(typeof(AffectedTargetComponent))]
//     public partial struct AggregateStatModifierJob : IJobEntity
//     {
//         public ModifierAggregatorContainer ModifierAggregatorContainer;
//
//         [ReadOnly] public ComponentLookup<StatModifierData> StatModifierDataLookup;
//         void Execute(in AffectedTargetComponent affectedTarget, in DynamicBuffer<StatModifierEntityElement> statModifierEntityElementBuffers)
//         {
//             foreach (var statModifierElementEntity in statModifierEntityElementBuffers)
//             {
//                 var statModifierData = this.StatModifierDataLookup[statModifierElementEntity.Value];
//
//                 var dataAggregator = ModifierAggregatorContainer.TryGetItem(affectedTarget.Value, statModifierData.TargetStat);
//                 switch (statModifierData.ModifierOperator)
//                 {
//                     case ModifierOperatorType.Add:
//                         dataAggregator.Add += statModifierData.ModifierMagnitude;
//                         break;
//                     case ModifierOperatorType.Multiply:
//                         dataAggregator.Multiply *= statModifierData.ModifierMagnitude;
//                         break;
//                     case ModifierOperatorType.Division:
//                         dataAggregator.Division *= statModifierData.ModifierMagnitude;
//                         break;
//                     case ModifierOperatorType.Override:
//                         dataAggregator.Override = statModifierData.ModifierMagnitude;
//                         break;
//                 }
//
//                 this.ModifierAggregatorContainer.TrySetItem(affectedTarget.Value, statModifierData.TargetStat, dataAggregator);
//             }
//         }
//     }
//
//     public struct ModifierAggregatorContainer
//     {
//         // ReSharper disable once InconsistentNaming
//         public NativeParallelHashMap<FixedString64Bytes, ModifierAggregatorData> EntityAffectedTarget_StatNameToModifierAggregators;
//         public NativeMultiHashMap<Entity, ModifierAggregatorData>                EntityAffectedTargetToMultiModifierAggregators;
//
//         public ModifierAggregatorContainer(int capacity)
//         {
//             this.EntityAffectedTarget_StatNameToModifierAggregators = new NativeParallelHashMap<FixedString64Bytes, ModifierAggregatorData>(capacity, Allocator.Persistent);
//             this.EntityAffectedTargetToMultiModifierAggregators     = new NativeMultiHashMap<Entity, ModifierAggregatorData>(capacity, Allocator.Persistent);
//         }
//
//         public void TrySetItem(Entity targetEntity, FixedString64Bytes statName, ModifierAggregatorData aggregatorData)
//         {
//             this.EntityAffectedTarget_StatNameToModifierAggregators[GetKey(targetEntity, statName)] = aggregatorData;
//             this.EntityAffectedTargetToMultiModifierAggregators.Add(targetEntity, aggregatorData);
//         }
//
//         public ModifierAggregatorData TryGetItem(Entity targetEntity, FixedString64Bytes statName)
//         {
//             if (this.EntityAffectedTarget_StatNameToModifierAggregators.TryGetValue(GetKey(targetEntity, statName), out var dataAggregator)) return dataAggregator;
//             return new ModifierAggregatorData()
//             {
//                 TargetStat = statName,
//                 Add        = 0,
//                 Multiply   = 1,
//                 Division   = 1
//             };
//         }
//
//         public void Dispose(JobHandle inputDeps)
//         {
//             EntityAffectedTarget_StatNameToModifierAggregators.Dispose(inputDeps);
//             EntityAffectedTargetToMultiModifierAggregators.Dispose(inputDeps);
//         }
//
//         private FixedString64Bytes GetKey(Entity targetEntity, FixedString64Bytes statName) => $"E{targetEntity.Index}:{targetEntity.Version}_{statName}";
//     }
// }