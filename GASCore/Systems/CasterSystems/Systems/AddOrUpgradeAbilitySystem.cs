namespace GASCore.Systems.CasterSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.CasterSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Transforms;

    [UpdateInGroup(typeof(GameAbilityInitializeSystemGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct AddOrUpgradeAbilitySystem : ISystem
    {
        private EntityQuery requestAddAbilityQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginInitializationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<AbilityEntityPrefabPoolSystem.Singleton>();

            this.requestAddAbilityQuery = SystemAPI.QueryBuilder().WithAll<RequestAddOrUpgradeAbility>().WithOptions(EntityQueryOptions.IncludePrefab).Build();
            this.requestAddAbilityQuery.SetChangedVersionFilter(ComponentType.ReadOnly<RequestAddOrUpgradeAbility>());
            state.RequireForUpdate(this.requestAddAbilityQuery);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (this.requestAddAbilityQuery.IsEmpty) return;

            new AddOrUpgradeAbilityJob()
            {
                AbilityEntityPrefabPool    = SystemAPI.GetSingleton<AbilityEntityPrefabPoolSystem.Singleton>().AbilityNameToLevelPrefabs,
                ChildLookup                = SystemAPI.GetBufferLookup<Child>(true),
                LinkedEntityGroupLookup    = SystemAPI.GetBufferLookup<LinkedEntityGroup>(true),
                AbilityContainerLookup     = SystemAPI.GetBufferLookup<AbilityContainerElement>(true),
                RequestRemoveAbilityLookup = SystemAPI.GetBufferLookup<RequestRemoveAbility>(true),
                ECB                        = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
            }.ScheduleParallel(this.requestAddAbilityQuery);
        }

        [BurstCompile]
        [WithChangeFilter(typeof(RequestAddOrUpgradeAbility))]
        [WithOptions(EntityQueryOptions.IncludePrefab)]
        public partial struct AddOrUpgradeAbilityJob : IJobEntity
        {
            [ReadOnly] public BufferLookup<LinkedEntityGroup>           LinkedEntityGroupLookup;
            [ReadOnly] public BufferLookup<Child>                       ChildLookup;
            [ReadOnly] public BufferLookup<AbilityContainerElement>     AbilityContainerLookup;
            [ReadOnly] public BufferLookup<RequestRemoveAbility>        RequestRemoveAbilityLookup;
            [ReadOnly] public NativeHashMap<FixedString64Bytes, Entity> AbilityEntityPrefabPool;
            public            EntityCommandBuffer.ParallelWriter        ECB;
            public void Execute(Entity casterEntity, [EntityIndexInQuery] int entityInQueryIndex, ref DynamicBuffer<RequestAddOrUpgradeAbility> requestAddOrUpgradeAbilities)
            {
                if (requestAddOrUpgradeAbilities.Length <= 0) return;
                // try get ability container buffer
                if (!this.AbilityContainerLookup.TryGetBuffer(casterEntity, out var abilityContainerBuffer))
                    abilityContainerBuffer = this.ECB.AddBuffer<AbilityContainerElement>(entityInQueryIndex, casterEntity);

                // try get linked group and child group to setup this ability to child of caster ability on hierarchy
                if (!this.LinkedEntityGroupLookup.HasBuffer(casterEntity))
                {
                    var linkedGroup = this.ECB.AddBuffer<LinkedEntityGroup>(entityInQueryIndex, casterEntity);
                    linkedGroup.Add(new LinkedEntityGroup() { Value = casterEntity });
                }

                if (!this.ChildLookup.HasBuffer(casterEntity)) this.ECB.AddBuffer<Child>(entityInQueryIndex, casterEntity);


                this.FilterRequest(casterEntity, entityInQueryIndex, ref requestAddOrUpgradeAbilities, abilityContainerBuffer);

                foreach (var request in requestAddOrUpgradeAbilities)
                {
                    // try to add new ability flow
                    // Debug.Log($"requestInitializeAbility + {requestAddOrUpgradeAbility.AbilityLevelKey} ");
                    if (this.AbilityEntityPrefabPool.TryGetValue(request.AbilityLevelKey, out var abilityPrefab))
                    {
                        // instantiate ability and log to AbilityContainerElement
                        InstantiateAbilityEntityPrefabSystem.InstantiateAbilityEntity(this.ECB, entityInQueryIndex, casterEntity, abilityPrefab, request);
                    }
                    else
                    {
                        var entity = this.ECB.CreateEntity(entityInQueryIndex);
                        this.ECB.AddComponent(entityInQueryIndex, entity, new LoadAbilityPrefabData()
                        {
                            CasterEntity = casterEntity,
                            RequestData  = request
                        });
                    }
                }

                requestAddOrUpgradeAbilities.Clear();
            }
          

            private void FilterRequest(Entity casterEntity, int entityInQueryIndex,
                ref DynamicBuffer<RequestAddOrUpgradeAbility> requestAddOrUpgradeAbilities, DynamicBuffer<AbilityContainerElement> abilityContainerBuffer)
            {
                var currentAbilityMap = new NativeHashMap<FixedString64Bytes, int>(requestAddOrUpgradeAbilities.Length, Allocator.Temp);
                foreach (var abilityContainerElement in abilityContainerBuffer)
                {
                    currentAbilityMap.Add(abilityContainerElement.AbilityId, abilityContainerElement.Level);
                }

                bool isAddedRequestRemoveBuffer = false;
                for (var index = 0; index < requestAddOrUpgradeAbilities.Length;)
                {
                    var request = requestAddOrUpgradeAbilities[index];
                    if (currentAbilityMap.TryGetValue(request.AbilityId, out var currentLevel))
                    {
                        if (request.Level <= currentLevel)
                        {
                            // remove if this request is duplicate with same abilityId and lower level
                            requestAddOrUpgradeAbilities.RemoveAtSwapBack(index);
                            continue;
                        }

                        // request remove the out date ability 
                        if (!isAddedRequestRemoveBuffer && !this.RequestRemoveAbilityLookup.HasBuffer(casterEntity))
                        {
                            isAddedRequestRemoveBuffer = true;
                            this.ECB.AddBuffer<RequestRemoveAbility>(entityInQueryIndex, casterEntity);
                        }

                        // UnityEngine.Debug.Log($"Request remove ability {request.AbilityId.Value}_Lv{currentLevel}");
                        this.ECB.AppendToBuffer(entityInQueryIndex, casterEntity, new RequestRemoveAbility(request.AbilityId, currentLevel));
                    }

                    currentAbilityMap[request.AbilityId] = request.Level;
                    index++;
                }

                currentAbilityMap.Dispose();
            }
        }
    }
}