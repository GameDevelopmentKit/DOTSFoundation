namespace GASCore.Systems.CasterSystems.Systems
{
    using DOTSCore.Extension;
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.AbilityMainFlow.Systems.AbilitySystem;
    using GASCore.Systems.CasterSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Transforms;
    using UnityEngine;

    [UpdateInGroup(typeof(AbilityCleanupSystemGroup))]
    [UpdateBefore(typeof(CleanupUnusedAbilityEntitiesSystem))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial class AddOrUpgradeAbilitySystem : SystemBase
    {
        private BeginInitializationEntityCommandBufferSystem beginInitEcbSystem;

        [NativeDisableParallelForRestriction] private BufferLookup<AbilityContainerElement> abilityContainerLookup;

        protected override void OnCreate()
        {
            this.RequireForUpdate<AbilityPrefabPool>();
            abilityContainerLookup = SystemAPI.GetBufferLookup<AbilityContainerElement>(true);
            this.beginInitEcbSystem = this.World.GetExistingSystemManaged<BeginInitializationEntityCommandBufferSystem>();
        }


        protected override void OnUpdate()
        {
            var ecb                       = this.beginInitEcbSystem.CreateCommandBuffer().AsParallelWriter();
            var abilityNameToLevelPrefabs = SystemAPI.GetSingleton<AbilityPrefabPool>().AbilityNameToLevelPrefabs.Value;
            var temp                      = abilityContainerLookup.UpdateBufferLookup(this);

            // Manage adding ability flow
            Entities.WithBurst().WithEntityQueryOptions(EntityQueryOptions.IncludePrefab).WithReadOnly(temp).WithReadOnly(abilityNameToLevelPrefabs).WithChangeFilter<RequestAddOrUpgradeAbility>().ForEach(
                (Entity casterEntity, int entityInQueryIndex, ref DynamicBuffer<RequestAddOrUpgradeAbility> requestAddOrUpgradeAbilities) =>
                {
                    // try get ability container buffer
                    if (!temp.TryGetBuffer(casterEntity, out var abilityContainerBuffer))
                        abilityContainerBuffer = ecb.AddBuffer<AbilityContainerElement>(entityInQueryIndex, casterEntity);

                    var listAbilityEntity = new NativeList<Entity>(Allocator.Temp);

                    foreach (var requestAddOrUpgradeAbility in requestAddOrUpgradeAbilities)
                    {
                        bool isExist = false; //is true if this found the same ability (name, level)
                        
                        // request remove old ability if exist
                        foreach (var abilityContainerElement in abilityContainerBuffer)
                        {
                            if (!requestAddOrUpgradeAbility.AbilityId.Equals(abilityContainerElement.AbilityId)) continue;
                            //skip this request if caster already have this ability with same level
                            // if not request remove the old one to add new one
                            if (requestAddOrUpgradeAbility.Level == abilityContainerElement.Level)
                            {
                                isExist = true;
                            }
                            else
                            {
                                if (!HasBuffer<RequestRemoveAbility>(casterEntity)) ecb.AddBuffer<RequestRemoveAbility>(entityInQueryIndex, casterEntity);

                                Debug.Log($"Request remove ability {abilityContainerElement.AbilityId}_Lv{abilityContainerElement.Level}");
                                ecb.AppendToBuffer(entityInQueryIndex, casterEntity, new RequestRemoveAbility(abilityContainerElement.AbilityId, abilityContainerElement.Level));   

                            }
                            break;
                        }

                        if (!isExist)
                        {
                            
                            // try to add new ability flow
                            // Debug.Log($"requestInitializeAbility + {requestAddOrUpgradeAbility.AbilityLevelKey} ");
                            if (abilityNameToLevelPrefabs.TryGetValue(requestAddOrUpgradeAbility.AbilityLevelKey, out var abilityPrefab))
                            {
                                // instantiate ability and log to AbilityContainerElement
                                var abilityEntity = ecb.Instantiate(entityInQueryIndex, abilityPrefab);

                                if (requestAddOrUpgradeAbility.IsPrefab)
                                {
                                    ecb.AddComponent<Prefab>(entityInQueryIndex, abilityEntity);
                                }

                                ecb.AppendToBuffer(entityInQueryIndex, casterEntity, new AbilityContainerElement()
                                {
                                    AbilityId       = requestAddOrUpgradeAbility.AbilityId,
                                    Level           = requestAddOrUpgradeAbility.Level,
                                    AbilityInstance = abilityEntity
                                });

                                ecb.AddComponent(entityInQueryIndex, abilityEntity, new CasterComponent() { Value = casterEntity });
                                ecb.SetParent(entityInQueryIndex, abilityEntity, casterEntity);
                                listAbilityEntity.Add(abilityEntity);
                            }
                            else
                            {
                                Debug.LogError($"Ability {requestAddOrUpgradeAbility.AbilityLevelKey} is not found in Pool. Please recheck this Id in AbilityBlueprint");
                                return;
                            }
                        }
                    }

                    requestAddOrUpgradeAbilities.Clear();

                    // try get linked group and child group to setup this ability to child of caster ability on hierarchy
                    if (!HasBuffer<LinkedEntityGroup>(casterEntity))
                    {
                        var linkedGroup = ecb.AddBuffer<LinkedEntityGroup>(entityInQueryIndex, casterEntity);
                        linkedGroup.Add(new LinkedEntityGroup() { Value = casterEntity });
                    }

                    if (!HasBuffer<Child>(casterEntity)) ecb.AddBuffer<Child>(entityInQueryIndex, casterEntity);

                    foreach (var abilityEntity in listAbilityEntity)
                    {
                        ecb.AppendToBuffer(entityInQueryIndex, casterEntity, new LinkedEntityGroup() { Value = abilityEntity });
                    }

                    listAbilityEntity.Dispose();
                }).ScheduleParallel();
            
            this.beginInitEcbSystem.AddJobHandleForProducer(this.Dependency);
        }
    }
}