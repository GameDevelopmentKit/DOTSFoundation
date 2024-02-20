namespace GASCore.Systems.CasterSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.CasterSystems.Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;

    [UpdateInGroup(typeof(GameAbilityInitializeSystemGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct RemoveAbilitySystemSystem : ISystem
    {
        EntityQuery requestRemoveAbilityQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginInitializationEntityCommandBufferSystem.Singleton>();
            this.requestRemoveAbilityQuery = SystemAPI.QueryBuilder().WithAll<RequestRemoveAbility, AbilityContainerElement>().Build();
            this.requestRemoveAbilityQuery.SetChangedVersionFilter(ComponentType.ReadOnly<RequestRemoveAbility>());
            state.RequireForUpdate(this.requestRemoveAbilityQuery);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (this.requestRemoveAbilityQuery.IsEmpty) return;

            new RemoveAbilityJob()
            {
                ECB = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
            }.ScheduleParallel(this.requestRemoveAbilityQuery);
        }

        [BurstCompile]
        [WithChangeFilter(typeof(RequestRemoveAbility))]
        public partial struct RemoveAbilityJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter ECB;
            void Execute([EntityIndexInQuery] int entityInQueryIndex, ref DynamicBuffer<RequestRemoveAbility> requestRemoveAbilities, ref DynamicBuffer<AbilityContainerElement> abilityContainerBuffer)
            {
                foreach (var requestRemoveAbility in requestRemoveAbilities)
                {
                    // Debug.Log($"remove ability {requestRemoveAbility.AbilityId}_Lv{requestRemoveAbility.Level}");
                    for (var index = 0; index < abilityContainerBuffer.Length; index++)
                    {
                        var abilityContainerElement = abilityContainerBuffer[index];
                        if (!requestRemoveAbility.AbilityId.Equals(abilityContainerElement.AbilityId)) continue;
                        if (requestRemoveAbility.Level == -1 || requestRemoveAbility.Level == abilityContainerElement.Level)
                        {
                            abilityContainerBuffer.RemoveAtSwapBack(index);
                            index = math.max(index - 1, 0);
                            this.ECB.DestroyEntity(entityInQueryIndex, abilityContainerElement.AbilityInstance);
                        }
                    }
                }

                requestRemoveAbilities.Clear();
            }
        }
    }
}