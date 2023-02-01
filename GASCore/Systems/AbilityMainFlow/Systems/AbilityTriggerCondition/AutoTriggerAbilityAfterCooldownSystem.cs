namespace GASCore.Systems.AbilityMainFlow.Systems.AbilityTriggerCondition
{
    using GASCore.Groups;
    using GASCore.Services;
    using GASCore.Systems.AbilityMainFlow.Components;
    using Unity.Burst;
    using Unity.Entities;
    using UnityEngine;
    using CountdownTimeSystem = GASCore.Systems.AbilityMainFlow.Systems.AbilitySystem.CountdownTimeSystem;

    [UpdateInGroup(typeof(AbilityMainFlowGroup))]
    [UpdateAfter(typeof(CountdownTimeSystem))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct AutoTriggerAbilityAfterCooldownSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            new AutoTriggerAbilityAfterCooldownJob()
            {
                Ecb = ecb,
            }.ScheduleParallel();
        }
        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }

    [BurstCompile]
    [WithAll(typeof(AbilityId), typeof(Cooldown), typeof(AutoActiveAfterCooldownTag))]
    [WithNone(typeof(Duration))]
    public partial struct AutoTriggerAbilityAfterCooldownJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;

        void Execute(Entity abilityEntity, [EntityInQueryIndex] int entityInQueryIndex)
        {
            // Debug.Log($"AutoTriggerAbilityAfterCooldownJob ability Index {abilityEntity.Index}");
            this.Ecb.MarkTriggerConditionComplete<AutoActiveAfterCooldownTag>(abilityEntity, entityInQueryIndex);
        }
    }
}