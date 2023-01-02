namespace GASCore.Systems.AbilityMainFlow.Systems.AbilityTriggerCondition
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.TimelineSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using UnityEngine;
    using CountdownTimeSystem = GASCore.Systems.AbilityMainFlow.Systems.AbilitySystem.CountdownTimeSystem;

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
            new AutoTriggerAbility1Job()
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
    public partial struct AutoTriggerAbility1Job : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;

        void Execute(Entity abilityEntity, [EntityInQueryIndex] int entityInQueryIndex, in TriggerConditionCount triggerConditionCount)
        {
            // Debug.Log($"AutoTriggerAbility1Job ability Index {abilityEntity.Index}");
            this.Ecb.SetComponent(entityInQueryIndex, abilityEntity, new TriggerConditionCount() { Value = triggerConditionCount.Value - 1 });
        }
    }
}