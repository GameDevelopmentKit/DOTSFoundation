namespace GASCore.Services
{
    using GASCore.Blueprints;
    using GASCore.Systems.LogicEffectSystems.Components;
    using GASCore.Systems.TargetDetectionSystems.Components;
    using GASCore.Systems.TimelineSystems.Components;
    using Unity.Collections;
    using Unity.Entities;
    using UnityEngine;

    public static class AbilityHelper
    {
        public static void AddAllAffectedTarget(
            ref this NativeHashSet<Entity> result,
            in Entity caster,
            in DynamicBuffer<TargetableElement> targetableEntities,
            in DynamicBuffer<AffectedTargetTypeElement> affectedTargetTypes,
            in ComponentLookup<TeamOwnerId> team)
        {
            // filter entity in targetableEntities  by affected target types
            foreach (var currentTargetType in affectedTargetTypes)
            {
                switch (currentTargetType.Value)
                {
                    case TargetType.None:
                        break;
                    case TargetType.Caster:
                        result.Add(caster);
                        break;
                    case TargetType.Opponent:
                        foreach (var targetElement in targetableEntities)
                        {
                            if (!team.HasComponent(targetElement.Value))
                            {
                                Debug.Log($"missing team component - {targetElement.Value}");
                                continue;
                            }
                            if (team[caster].Value != team[targetElement.Value].Value)
                                result.Add(targetElement.Value);
                        }
                        
                        //DON'T REMOVE THIS LOG, it's used to fix a magic bug on release build
                        Debug.Log($"AddAllAffectedTargetInAoe - targetBuffer count = {result.Count}");

                        break;
                    case TargetType.Ally:
                        foreach (var targetElement in targetableEntities)
                        {
                            if (!team.HasComponent(targetElement.Value))
                            {
                                Debug.Log($"missing team component - {targetElement.Value}");
                                continue;
                            }
                            if (team[caster].Value == team[targetElement.Value].Value)
                                result.Add(targetElement.Value);
                        }

                        break;
                }
            }
        }

        public static void MarkTriggerConditionComplete<T>(this EntityCommandBuffer.ParallelWriter ecb, Entity triggerEntity, int entityInQueryIndex)
        {
            ecb.AppendToBuffer(entityInQueryIndex, triggerEntity, new CompletedTriggerElement() { Index = TypeManager.GetTypeIndex<T>() });
        }
    }
}