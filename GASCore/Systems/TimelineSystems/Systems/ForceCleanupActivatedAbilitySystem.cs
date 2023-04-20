namespace GASCore.Systems.TimelineSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.TimelineSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using UnityEngine;

    [UpdateInGroup(typeof(AbilityTimelineGroup))]
    [UpdateAfter(typeof(TrackingTriggerConditionProgressSystem))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct ForceCleanupActivatedAbilitySystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            using var entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<ForceCleanupActivatedAbilityTag,CompletedAllTriggerConditionTag>();
            state.RequireForUpdate(state.GetEntityQuery(entityQuery));
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton    = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb             = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            new ForceCleanupActivatedAbilityJob()
            {
                Ecb                = ecb,
                LinkedEntityBufferLookup = SystemAPI.GetBufferLookup<LinkedEntityGroup>(true)
            }.ScheduleParallel();
        }
    }

    [BurstCompile]
    [WithAll(typeof(ForceCleanupActivatedAbilityTag), typeof(CompletedAllTriggerConditionTag))]
    public partial struct ForceCleanupActivatedAbilityJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;

        [ReadOnly] public BufferLookup<LinkedEntityGroup> LinkedEntityBufferLookup;
        void Execute([EntityIndexInQuery] int entityInQueryIndex, in ActivatedStateEntityOwner activatedStateEntityOwner)
        {
            Debug.Log("ForceCleanupActivatedAbilityJob");
            if (this.LinkedEntityBufferLookup.TryGetBuffer(activatedStateEntityOwner.Value, out var linkedEntityGroups))
            {
                foreach (var linkedEntity in linkedEntityGroups)
                {
                    this.Ecb.AddComponent<ForceCleanupTag>(entityInQueryIndex, linkedEntity.Value);
                }
            }
        }
    }
}