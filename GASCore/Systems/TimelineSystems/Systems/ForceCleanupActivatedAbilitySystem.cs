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
        private BufferLookup<LinkedEntityGroup> linkedEntityBufferLookup;
        [BurstCompile]
        public void OnCreate(ref SystemState state) { this.linkedEntityBufferLookup = state.GetBufferLookup<LinkedEntityGroup>(true); }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            this.linkedEntityBufferLookup.Update(ref state);
            var ecbSingleton    = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb             = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            var setEndTimeTriggerAfterSecondJob = new ForceCleanupActivatedAbilityJob()
            {
                Ecb                = ecb,
                LinkedEntityBufferLookup = this.linkedEntityBufferLookup
            };
            setEndTimeTriggerAfterSecondJob.ScheduleParallel();
        }
    }

    [BurstCompile]
    [WithAll(typeof(ForceCleanupActivatedAbilityTag), typeof(CompletedAllTriggerConditionTag))]
    public partial struct ForceCleanupActivatedAbilityJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;

        [ReadOnly] public BufferLookup<LinkedEntityGroup> LinkedEntityBufferLookup;
        void Execute([EntityInQueryIndex] int entityInQueryIndex, in ActivatedStateEntityOwner activatedStateEntityOwner)
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