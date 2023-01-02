namespace GASCore.Systems.TimelineSystems.Systems
{
    using DOTSCore.Extension;
    using GASCore.Groups;
    using GASCore.Services;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using GASCore.Systems.TimelineSystems.Components;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Transforms;
    using UnityEngine;

    [UpdateInGroup(typeof(AbilityTimelineGroup))]
    [UpdateAfter(typeof(DisableTriggerConditionCountSystem))]
    [RequireMatchingQueriesForUpdate]
    public partial class InstantiateAbilityEffectFromPoolSystem : SystemBase
    {
        ComponentLookup<AoE>                     aoeComponentLookup;
        BufferLookup<AffectedTargetTypeElement>  affectedTargetTypeLookup;
        BufferLookup<Child>                      childLookup;
        BufferLookup<AbilityEffectPoolComponent> effectPoolLookup;

        protected override void OnCreate()
        {
            aoeComponentLookup       = this.GetComponentLookup<AoE>(true);
            affectedTargetTypeLookup = this.GetBufferLookup<AffectedTargetTypeElement>(true);
            childLookup              = this.GetBufferLookup<Child>(true);
            effectPoolLookup         = this.GetBufferLookup<AbilityEffectPoolComponent>(true);
        }

        protected override void OnUpdate()
        {
            EntityCommandBuffer                ecb         = new EntityCommandBuffer(Allocator.TempJob);
            EntityCommandBuffer.ParallelWriter ecbParallel = ecb.AsParallelWriter();

            var localAoeLookup            = aoeComponentLookup.UpdateComponentLookup(this);
            var localAffectedTargetLookup = affectedTargetTypeLookup.UpdateBufferLookup(this);
            var localChildLookup          = childLookup.UpdateBufferLookup(this);
            var localEffectPoolLookup     = effectPoolLookup.UpdateBufferLookup(this);

            Entities.WithAll<WaitingCreateEffect>().WithNone<TriggerConditionCount>()
                .WithReadOnly(localAoeLookup).WithReadOnly(localAffectedTargetLookup).WithReadOnly(localChildLookup).WithReadOnly(localEffectPoolLookup)
                .ForEach((Entity actionEntity, int entityInQueryIndex, ref DynamicBuffer<TargetElement> targetBuffer, ref DynamicBuffer<ExcludeAffectedTargetElement> excludeAffectedTargetBuffer,
                    in DynamicBuffer<CreateAbilityEffectElement> abilityEffectIds,
                    in ActivatedStateEntityOwner activatedAbilityEntity, in CasterComponent caster) =>
                {
                    var effectPoolBuffer       = localEffectPoolLookup[activatedAbilityEntity.Value];
                    var effectIdToEffectPrefab = new NativeHashMap<FixedString64Bytes, Entity>(effectPoolBuffer.Length, Allocator.Temp);

                    foreach (var effectPoolComponent in effectPoolBuffer)
                    {
                        var effectPrefab = effectPoolComponent.EffectPrefab;
                        effectIdToEffectPrefab.Add(GetComponent<AbilityEffectId>(effectPrefab).Value, effectPrefab);
                    }

                    foreach (var effectId in abilityEffectIds)
                    {
                        if (effectIdToEffectPrefab.TryGetValue(effectId.EffectId, out var effectEntityPrefab))
                        {
                            var affectTargetTypes          = localAffectedTargetLookup[effectEntityPrefab];
                            var effectActionPrefabEntities = localChildLookup[effectEntityPrefab];

                            //Find all affected targets
                            NativeHashSet<Entity> affectedTargetEntities = new NativeHashSet<Entity>(targetBuffer.Capacity, Allocator.Temp);

                            if (!HasComponent<AffectedTargetComponent>(actionEntity))
                            {
                                // take target aoe from effect entity if have, or take from ability owner
                                var abilityTargetAoe = localAoeLookup.HasComponent(actionEntity)
                                    ? localAoeLookup[actionEntity] // use for casting on multiple aoe purpose
                                    : localAoeLookup[activatedAbilityEntity.Value];

                                affectedTargetEntities.AddAllAffectedTargetInAoe(caster.Value, targetBuffer, abilityTargetAoe, affectTargetTypes);
                            }
                            else
                            {
                                // effect entity already has affected target, take this one
                                affectedTargetEntities.Add(GetComponent<AffectedTargetComponent>(actionEntity).Value);
                            }

                            //Filter exclude affected target
                            foreach (var excludeAffectedTargetElement in excludeAffectedTargetBuffer)
                            {
                                affectedTargetEntities.Remove(excludeAffectedTargetElement.Value);
                            }

                            foreach (var affectedTargetEntity in affectedTargetEntities)
                            {
                                //Debug.LogError($"InstantiateAbilityEffectFromPoolSystem {effectId.EffectId} for affectedTargetEntity {affectedTargetEntity.Index}");
                                foreach (var effectActionPrefab in effectActionPrefabEntities)
                                {
                                    var effectActionEntity = ecbParallel.Instantiate(entityInQueryIndex, effectActionPrefab.Value);
                                    ecbParallel.AddComponent(entityInQueryIndex, effectActionEntity, new AbilityEffectId() { Value           = effectId.EffectId });
                                    ecbParallel.AddComponent(entityInQueryIndex, effectActionEntity, new AffectedTargetComponent() { Value   = affectedTargetEntity });
                                    ecbParallel.AddComponent(entityInQueryIndex, effectActionEntity, new ActivatedStateEntityOwner() { Value = activatedAbilityEntity.Value });
                                    ecbParallel.AddComponent(entityInQueryIndex, effectActionEntity, caster);
                                    ecbParallel.RemoveParent(entityInQueryIndex, effectActionEntity);
                                    ecbParallel.AppendToBuffer(entityInQueryIndex, activatedAbilityEntity.Value, new LinkedEntityGroup() { Value = effectActionEntity });
                                }
                            }

                            affectedTargetEntities.Dispose();
                        }
                    }

                    effectIdToEffectPrefab.Dispose();
                    excludeAffectedTargetBuffer.Clear();
                    targetBuffer.Clear();
                    ecbParallel.SetComponentEnabled<WaitingCreateEffect>(entityInQueryIndex, actionEntity, false);
                }).ScheduleParallel();

            this.Dependency.Complete();
            ecb.Playback(this.EntityManager);
            ecb.Dispose();
        }
    }
}