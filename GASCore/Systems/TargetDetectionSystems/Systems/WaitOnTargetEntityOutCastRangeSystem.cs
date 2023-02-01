namespace GASCore.Systems.TargetDetectionSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Services;
    using GASCore.Systems.TargetDetectionSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using UnityEngine;

    [UpdateInGroup(typeof(AbilityTimelineGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct WaitOnTargetEntityOutCastRangeSystem : ISystem
    {
        private ComponentLookup<TriggerOnOutAbilityRange> triggerOnOutAbilityRangeLookup;
        private BufferLookup<LinkedEntityGroup>           linkedEntityLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            this.triggerOnOutAbilityRangeLookup  = state.GetComponentLookup<TriggerOnOutAbilityRange>(true);
            this.linkedEntityLookup              = state.GetBufferLookup<LinkedEntityGroup>(true);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            this.triggerOnOutAbilityRangeLookup.Update(ref state);
            this.linkedEntityLookup.Update(ref state);

            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            var waitOnTargetEntityOutCastRangeJob = new WaitOnTargetEntityOutCastRangeJob()
            {
                Ecb                             = ecb,
                LinkedEntityLookup              = this.linkedEntityLookup,
                TriggerOnOutAbilityRangeLookup  = this.triggerOnOutAbilityRangeLookup
            };

            state.Dependency = waitOnTargetEntityOutCastRangeJob.ScheduleParallel(state.Dependency);
        }
    }

    [BurstCompile]
    public partial struct WaitOnTargetEntityOutCastRangeJob : IJobEntity
    {
        public            EntityCommandBuffer.ParallelWriter        Ecb;
        [ReadOnly] public ComponentLookup<TriggerOnOutAbilityRange> TriggerOnOutAbilityRangeLookup;
        [ReadOnly] public BufferLookup<LinkedEntityGroup>           LinkedEntityLookup;
        void Execute([EntityInQueryIndex] int entityInQueryIndex, in OnOutAbilityRange onOutAbilityRange)
        {
            // Debug.Log($"WaitOnTargetEntityOutCastRangeJob onOutAbilityRange.TargetEntity.Index = {onOutAbilityRange.TargetEntity.Index}");

            if (this.LinkedEntityLookup.TryGetBuffer(onOutAbilityRange.ActivatedStateAbilityEntity, out var linkedEntityGroups))
            {
                foreach (var linkedEntity in linkedEntityGroups)
                {
                    if (this.TriggerOnOutAbilityRangeLookup.HasComponent(linkedEntity.Value))
                    {
                        // Debug.Log($"WaitOnTargetEntityOutCastRangeJob  {linkedEntity.Value.Index}");
                        //set target entity 
                        this.Ecb.AppendToBuffer(entityInQueryIndex, linkedEntity.Value, new TargetableElement() { Value = onOutAbilityRange.TargetEntity });

                        //mark this condition was done
                        this.Ecb.MarkTriggerConditionComplete<TriggerOnOutAbilityRange>(linkedEntity.Value, entityInQueryIndex);
                    }
                }
            }
        }
    }
}