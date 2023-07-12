namespace GASCore.Systems.TargetDetectionSystems.Systems
{
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.TargetDetectionSystems.Components;
    using GASCore.Systems.TargetDetectionSystems.Components.Filters;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;

    [UpdateInGroup(typeof(FinalizeFindTargetGroup))]
    [UpdateAfter(typeof(FinalizeFindTargetSystem))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct DestroyEntityOnHitSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new DestroyEntityOnHitJob
            {
                ForceCleanupComponentLookup = SystemAPI.GetComponentLookup<ForceCleanupTag>()
            }.ScheduleParallel();
        }
    }

    [WithAll(typeof(FilterOnHit))]
    [BurstCompile]
    public partial struct DestroyEntityOnHitJob : IJobEntity
    {
        [NativeDisableParallelForRestriction] public ComponentLookup<ForceCleanupTag> ForceCleanupComponentLookup;

        private void Execute(in DynamicBuffer<TargetableElement> targetables, ref DynamicBuffer<CacheTriggerEventElement> cacheTriggerEventBuffer)
        {
            foreach (var cacheTriggerEventElement in cacheTriggerEventBuffer)
            {
                foreach (var targetableElement in targetables)
                {
                    if (!targetableElement.Value.Equals(cacheTriggerEventElement.OtherEntity)) continue;
                    if (this.ForceCleanupComponentLookup.HasComponent(cacheTriggerEventElement.SourceEntity))
                    {
                        this.ForceCleanupComponentLookup.SetComponentEnabled(cacheTriggerEventElement.SourceEntity, true);
                    }
                }
            }

            cacheTriggerEventBuffer.Clear();
        }
    }
}