namespace GASCore.Systems.CasterSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Systems.AbilitySystem;
    using GASCore.Systems.CasterSystems.Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;
    using UnityEngine;

    [UpdateInGroup(typeof(AbilityCleanupSystemGroup))]
    [UpdateAfter(typeof(CleanupUnusedAbilityEntitiesSystem))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial class RemoveAbilitySystemSystem : SystemBase
    {
        private BeginInitializationEntityCommandBufferSystem beginInitEcbSystem;


        protected override void OnCreate() { this.beginInitEcbSystem = this.World.GetExistingSystemManaged<BeginInitializationEntityCommandBufferSystem>(); }


        protected override void OnUpdate()
        {
            var ecb = this.beginInitEcbSystem.CreateCommandBuffer().AsParallelWriter();

            //Manage removing ability flow
            this.Entities.WithBurst().WithChangeFilter<RequestRemoveAbility>().ForEach(
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

            this.beginInitEcbSystem.AddJobHandleForProducer(this.Dependency);
        }
    }
}