namespace GASCore.Systems.CasterSystems.Systems
{
    using System;
    using DOTSCore.Extension;
    using GameFoundation.Scripts.Utilities.Extension;
    using GASCore.Blueprints;
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.AbilityMainFlow.Factories;
    using GASCore.Systems.CasterSystems.Components;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;
    using UnityEngine;
    using Zenject;

    [UpdateInGroup(typeof(AbilityMainFlowGroup))]
    [RequireMatchingQueriesForUpdate]
    public partial class AbilityPoolManagerSystem : SystemBase
    {
        private AbilityBlueprint           abilityBlueprint;
        private AbilityEntityPrefabFactory abilityPrefabFactory;

        private BeginSimulationEntityCommandBufferSystem beginSimEcbSystem;

        [NativeDisableParallelForRestriction] private BufferLookup<AbilityContainerElement> abilityContainerLookup;

        [Inject]
        public void Inject(AbilityBlueprint abilityBlueprintParam, AbilityEntityPrefabFactory abilityEntityPrefabFactory)
        {
            this.abilityBlueprint     = abilityBlueprintParam;
            this.abilityPrefabFactory = abilityEntityPrefabFactory;
            this.beginSimEcbSystem    = this.World.GetExistingSystemManaged<BeginSimulationEntityCommandBufferSystem>();
        }

        protected override void OnCreate()
        {
            this.RequireForUpdate<AbilityPrefabPool>();

            this.GetCurrentContainer().Inject(this);
            var abilityNameToLevelPrefabs = new NativeParallelHashMap<FixedString64Bytes, Entity>(this.abilityBlueprint.Count, Allocator.Persistent);
            var abilityPrefabHolderEntity = EntityManager.CreateEntity(EntityManager.CreateArchetype(typeof(LocalToWorld), typeof(Translation), typeof(Rotation)));
            EntityManager.SetName(abilityPrefabHolderEntity, "AbilityPrefabHolder");

            foreach (var abilityRecord in this.abilityBlueprint.Values)
            {
                foreach (var abilityLevelRecord in abilityRecord.LevelRecords)
                {
                    var abilityEntityPrefab = this.abilityPrefabFactory.CreateEntity(EntityManager, new AbilityFactoryModel()
                    {
                        AbilityRecord      = abilityRecord,
                        AbilityLevelRecord = abilityLevelRecord
                    });
                    var levelIndex = abilityLevelRecord.LevelIndex + 1;
                    abilityNameToLevelPrefabs.Add($"{abilityRecord.Id}_{levelIndex}", abilityEntityPrefab);

                    if (levelIndex == abilityRecord.LevelRecords.Count)
                    {
                        EntityManager.AddComponent<MaxLevelTag>(abilityEntityPrefab);
                    }

                    EntityManager.SetParent(abilityEntityPrefab, abilityPrefabHolderEntity);
                }
            }

            EntityManager.AddComponentData(abilityPrefabHolderEntity, new AbilityPrefabPool() { AbilityNameToLevelPrefabs = abilityNameToLevelPrefabs.CreateReference() });

            abilityContainerLookup = SystemAPI.GetBufferLookup<AbilityContainerElement>(true);
        }


        protected override void OnUpdate()
        {
            var ecb                       = this.beginSimEcbSystem.CreateCommandBuffer().AsParallelWriter();
            var abilityNameToLevelPrefabs = this.GetSingleton<AbilityPrefabPool>().AbilityNameToLevelPrefabs.Value;
            var temp                      = abilityContainerLookup.UpdateBufferLookup(this);

            // Manage adding ability flow
            Entities.WithEntityQueryOptions(EntityQueryOptions.IncludePrefab).WithReadOnly(temp).WithReadOnly(abilityNameToLevelPrefabs).WithChangeFilter<RequestAddOrUpgradeAbility>().ForEach(
                (Entity casterEntity, int entityInQueryIndex, ref DynamicBuffer<RequestAddOrUpgradeAbility> requestAddOrUpgradeAbilities) =>
                {
                    // try get ability container buffer
                    if (!temp.TryGetBuffer(casterEntity, out var abilityContainerBuffer))
                        abilityContainerBuffer = ecb.AddBuffer<AbilityContainerElement>(entityInQueryIndex, casterEntity);

                    var listAbilityEntity = new NativeList<Entity>(Allocator.Temp);
                    foreach (var requestAddOrUpgradeAbility in requestAddOrUpgradeAbilities)
                    {
                        // request remove old ability if exist
                        foreach (var abilityContainerElement in abilityContainerBuffer)
                        {
                            if (!requestAddOrUpgradeAbility.AbilityId.Equals(abilityContainerElement.AbilityId)) continue;
                            //skip this request if caster already have this ability with same level
                            // if not request remove the old one to add new one
                            if (requestAddOrUpgradeAbility.Level == abilityContainerElement.Level) return;

                            if (!HasBuffer<RequestRemoveAbility>(casterEntity)) ecb.AddBuffer<RequestRemoveAbility>(entityInQueryIndex, casterEntity);

                            Debug.Log($"Request remove ability {abilityContainerElement.AbilityId}_Lv{abilityContainerElement.Level}");
                            ecb.AppendToBuffer(entityInQueryIndex, casterEntity, new RequestRemoveAbility(abilityContainerElement.AbilityId, abilityContainerElement.Level));
                        }

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

            //Manage removing ability flow
            this.Entities.WithChangeFilter<RequestRemoveAbility>().ForEach(
                (int entityInQueryIndex, ref DynamicBuffer<RequestRemoveAbility> requestRemoveAbilities, ref DynamicBuffer<AbilityContainerElement> abilityContainerBuffer) =>
                {
                    foreach (var requestRemoveAbility in requestRemoveAbilities)
                    {
                        Debug.Log($"remove ability {requestRemoveAbility.AbilityId}_Lv{requestRemoveAbility.Level}");
                        for (var index = 0; index < abilityContainerBuffer.Length; index++)
                        {
                            var abilityContainerElement = abilityContainerBuffer[index];
                            if (!requestRemoveAbility.AbilityId.Equals(abilityContainerElement.AbilityId)) continue;
                            if (requestRemoveAbility.Level == abilityContainerElement.Level)
                            {
                                abilityContainerBuffer.RemoveAtSwapBack(index);
                                index = math.max(index - 1, 0);
                                ecb.DestroyEntity(entityInQueryIndex, abilityContainerElement.AbilityInstance);
                            }
                        }
                    }

                    requestRemoveAbilities.Clear();
                }).ScheduleParallel();

            this.beginSimEcbSystem.AddJobHandleForProducer(this.Dependency);
        }
    }
}