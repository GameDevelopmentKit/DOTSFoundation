namespace GASCore.Systems.TimelineSystems.Systems
{
    using DOTSCore.Extension;
    using GASCore.Groups;
    using GASCore.Services;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using GASCore.Systems.TargetDetectionSystems.Components;
    using GASCore.Systems.TimelineSystems.Components;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Transforms;

    [UpdateInGroup(typeof(AbilityMainFlowGroup))]
    [RequireMatchingQueriesForUpdate]
    public partial class InstantiateAbilityEffectFromPoolSystem : SystemBase
    {
        ComponentLookup<TeamOwnerId>            teamLookup;
        BufferLookup<AffectedTargetTypeElement> affectedTargetTypeLookup;
        BufferLookup<Child>                     childLookup;

        private EndInitializationEntityCommandBufferSystem endInitEcbSystem;
        protected override void OnCreate()
        {
            affectedTargetTypeLookup = this.GetBufferLookup<AffectedTargetTypeElement>(true);
            childLookup              = this.GetBufferLookup<Child>(true);
            teamLookup               = this.GetComponentLookup<TeamOwnerId>(true);

            this.endInitEcbSystem = this.World.GetExistingSystemManaged<EndInitializationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var ecbParallel = this.endInitEcbSystem.CreateCommandBuffer().AsParallelWriter();

            var localAffectedTargetLookup = affectedTargetTypeLookup.UpdateBufferLookup(this);
            var localChildLookup          = childLookup.UpdateBufferLookup(this);
            var localTeamLookup           = teamLookup.UpdateComponentLookup(this);

            Entities.WithAll<CompletedAllTriggerConditionTag>()
                .WithReadOnly(localAffectedTargetLookup)
                .WithReadOnly(localChildLookup)
                .WithReadOnly(localTeamLookup)
                .ForEach((Entity actionEntity,
                    int entityInQueryIndex,
                    ref DynamicBuffer<TargetableElement> targetableBuffer,
                    ref DynamicBuffer<ExcludeAffectedTargetElement> excludeAffectedTargetBuffer,
                    in DynamicBuffer<CreateAbilityEffectElement> abilityEffectIds,
                    in ActivatedStateEntityOwner activatedAbilityEntity,
                    in CasterComponent caster) =>
                {
                    var effectIdToEffectPrefab = SystemAPI.GetComponent<AbilityEffectPoolComponent>(activatedAbilityEntity.Value).BlobValue.Value.AsReadOnly();

                    foreach (var effectId in abilityEffectIds)
                    {
                        if (effectIdToEffectPrefab.TryGetValue(effectId.EffectId, out var effectEntityPrefab))
                        {
                            var affectTargetTypes          = localAffectedTargetLookup[effectEntityPrefab];
                            var effectActionPrefabEntities = localChildLookup[effectEntityPrefab];

                            //Find all affected targets
                            NativeHashSet<Entity> affectedTargetEntities = new NativeHashSet<Entity>(targetableBuffer.Capacity, Allocator.Temp);

                            if (!SystemAPI.HasComponent<AffectedTargetComponent>(actionEntity))
                            {
                                affectedTargetEntities.AddAllAffectedTarget(caster.Value, targetableBuffer, affectTargetTypes, localTeamLookup);
                            }
                            else
                            {
                                // effect entity already has affected target, take this one
                                affectedTargetEntities.Add(SystemAPI.GetComponent<AffectedTargetComponent>(actionEntity).Value);
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

                    excludeAffectedTargetBuffer.Clear();
                    targetableBuffer.Clear();
                }).ScheduleParallel();

            this.endInitEcbSystem.AddJobHandleForProducer(this.Dependency);
        }
    }
}