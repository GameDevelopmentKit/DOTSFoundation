namespace GASCore.Systems.AbilityMainFlow.Systems.AbilityTriggerCondition
{
    using GASCore.Groups;
    using GASCore.Services;
    using GASCore.Systems.AbilityMainFlow.Components;
    using Unity.Burst;
    using Unity.Entities;
    using UnityEngine;

    [UpdateInGroup(typeof(AbilityMainFlowGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct AutoTriggerAbilityOnStartSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            new AutoTriggerAbilityOnStartJob()
            {
                Ecb = ecb,
            }.ScheduleParallel();
        }
        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }

    [BurstCompile]
    [WithAll(typeof(AbilityId), typeof(AutoActiveOnStartTag))]
    [WithChangeFilter(typeof(AutoActiveOnStartTag))]
    public partial struct AutoTriggerAbilityOnStartJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;

        void Execute(Entity abilityEntity, [EntityInQueryIndex] int entityInQueryIndex)
        {
            // Debug.Log($"AutoTriggerAbilityOnStartJob ability Index {abilityEntity.Index}");
            this.Ecb.MarkTriggerConditionComplete<AutoActiveOnStartTag>(abilityEntity, entityInQueryIndex);
        }
    }
}