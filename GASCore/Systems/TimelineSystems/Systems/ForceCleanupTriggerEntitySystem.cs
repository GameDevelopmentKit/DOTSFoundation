namespace GASCore.Systems.TimelineSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.TimelineSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityTimelineGroup))]
    [UpdateAfter(typeof(TrackingTriggerConditionProgressSystem))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct ForceCleanupTriggerEntitySystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            using var entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<ForceCleanupTriggerEntity, CompletedAllTriggerConditionTag>();
            state.RequireForUpdate(state.GetEntityQuery(entityQuery));
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            new ForceCleanupTriggerEntityJob()
            {
                Ecb                      = ecb,
                LinkedEntityBufferLookup = SystemAPI.GetBufferLookup<LinkedEntityGroup>(true),
                TriggerIndexLookup       = SystemAPI.GetComponentLookup<TriggerIndex>(true)
            }.ScheduleParallel();
        }
    }

    [BurstCompile]
    [WithAll(typeof(CompletedAllTriggerConditionTag))]
    public partial struct ForceCleanupTriggerEntityJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;

        [ReadOnly] public BufferLookup<LinkedEntityGroup> LinkedEntityBufferLookup;
        [ReadOnly] public ComponentLookup<TriggerIndex> TriggerIndexLookup;
        void Execute([EntityIndexInQuery] int entityInQueryIndex, in ActivatedStateEntityOwner activatedStateEntityOwner, in ForceCleanupTriggerEntity forceCleanupTriggerEntity)
        {
            if (!this.LinkedEntityBufferLookup.TryGetBuffer(activatedStateEntityOwner.Value, out var linkedEntityGroups)) return;
            foreach (var linkedEntity in linkedEntityGroups)
            {
                if (!this.TriggerIndexLookup.TryGetComponent(linkedEntity.Value, out var triggerIndex) || forceCleanupTriggerEntity.TriggerIndex != triggerIndex.Value) continue;
                this.Ecb.AddComponent<ForceCleanupTag>(entityInQueryIndex, linkedEntity.Value);
            }
        }
    }
}