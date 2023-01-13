namespace GASCore.Services
{
    using GASCore.Blueprints;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.CommonSystems.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;
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

        // get all entity in AoE
        private static NativeHashSet<Entity> GetEntitiesInAoE(in DynamicBuffer<TargetableElement> targetableEntities, in NativeArray<Entity> entities, in AoE aoe,
            in ComponentLookup<LocalToWorld> localToWorld)
        {
            var listTargetInAoE = new NativeHashSet<Entity>(targetableEntities.Length, Allocator.Persistent);
            foreach (var targetableEntity in targetableEntities)
            {
                foreach (var entity in entities)
                {
                    if (aoe.AoEType == AoEType.Single)
                    {
                        if (CheckInRange(targetableEntity.Value, entity, localToWorld, 1))
                        {
                            listTargetInAoE.Add(entity);
                            break;
                        }
                    }
                    else if (aoe.AoEType == AoEType.Round)
                    {
                        if (CheckInRange(targetableEntity.Value, entity, localToWorld, aoe.AoERange))
                            listTargetInAoE.Add(entity);
                    }
                }
            }

            return listTargetInAoE;
        }

        private static bool CheckInRange(in Entity e1, in Entity e2, in ComponentLookup<LocalToWorld> localToWorld, float range)
        {
            var distancesq = math.distancesq(localToWorld[e1].Position, localToWorld[e2].Position);
            return distancesq <= range * range;
        }
    }
}