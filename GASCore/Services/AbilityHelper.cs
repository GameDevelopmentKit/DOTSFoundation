namespace GASCore.Services
{
    using GASCore.Blueprints;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using Unity.Collections;
    using Unity.Entities;
    using UnityEngine;

    public static class AbilityHelper
    {
        //TODO implement GetAllAffectedTargetInAoe
        public static void AddAllAffectedTargetInAoe(ref this NativeHashSet<Entity> result ,in Entity casterEntity,in DynamicBuffer<TargetElement> targetBuffer, in AoE targetAoE,
            in DynamicBuffer<AffectedTargetTypeElement> affectTargetTypes)
        {
            foreach (var currentTargetType in affectTargetTypes)
            {
                switch (currentTargetType.Value)
                {
                    case TargetType.Opponent:
                        foreach (var targetElement in targetBuffer)
                        {
                            result.Add(targetElement.Value);
                        }
                        Debug.Log($"AddAllAffectedTargetInAoe - targetBuffer count = {targetBuffer.Length}");

                        break;
                    case TargetType.Self:
                        result.Add(casterEntity);
                        break;
                    case TargetType.Ally:
                        foreach (var targetElement in targetBuffer)
                        {
                            result.Add(targetElement.Value);
                        }
                        break;
                }
            }
        }
    }
}