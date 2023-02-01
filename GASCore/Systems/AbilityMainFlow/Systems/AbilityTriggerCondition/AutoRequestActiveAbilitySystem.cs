namespace GASCore.Systems.AbilityMainFlow.Systems.AbilityTriggerCondition
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.TimelineSystems.Components;
    using Unity.Burst;
    using Unity.Entities;
    using UnityEngine;

    [UpdateInGroup(typeof(AbilityMainFlowGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct AutoRequestActiveAbilitySystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { }
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            new AutoRequestActiveAbilityJob()
            {
                Ecb = ecb,
            }.ScheduleParallel();
        }
        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }

    [BurstCompile]
    [WithAll(typeof(AbilityId), typeof(CompletedAllTriggerConditionTag))]
    [WithNone(typeof(ManualActiveTag), typeof(RequestActivate))]
    public partial struct AutoRequestActiveAbilityJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;

        void Execute(Entity abilityEntity, [EntityInQueryIndex] int entityInQueryIndex)
        {
            // Debug.Log($"AutoRequestActiveAbilityJob ability Index {abilityEntity.Index}");
            this.Ecb.SetComponentEnabled<CompletedAllTriggerConditionTag>(entityInQueryIndex, abilityEntity, false);
            this.Ecb.SetComponentEnabled<RequestActivate>(entityInQueryIndex, abilityEntity, true);
        }
    }
}