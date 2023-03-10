namespace GASCore.Systems.TargetDetectionSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Services;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.TargetDetectionSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using UnityEngine;

    [UpdateInGroup(typeof(AbilityTimelineGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct WaitOnTargetEntityInCastRangeSystem : ISystem
    {
        private ComponentLookup<TriggerOnInAbilityRange> triggerOnInAbilityRangeLookup;
        private BufferLookup<LinkedEntityGroup>          linkedEntityLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            this.triggerOnInAbilityRangeLookup   = state.GetComponentLookup<TriggerOnInAbilityRange>(true);
            this.linkedEntityLookup              = state.GetBufferLookup<LinkedEntityGroup>(true);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            this.triggerOnInAbilityRangeLookup.Update(ref state);
            this.linkedEntityLookup.Update(ref state);

            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            var setupAbilityTrackingEntityInCastRangeJob = new SetupAbilityTrackingEntityInCastRange()
            {
                Ecb = ecb,
            };
            var setupAbilityJobHandle = setupAbilityTrackingEntityInCastRangeJob.ScheduleParallel(state.Dependency);

            var waitOnTargetEntityInCastRangeJob = new WaitOnTargetEntityInCastRangeJob()
            {
                Ecb                             = ecb,
                LinkedEntityLookup              = this.linkedEntityLookup,
                TriggerOnInAbilityRangeLookup   = this.triggerOnInAbilityRangeLookup
            };

            state.Dependency = waitOnTargetEntityInCastRangeJob.ScheduleParallel(setupAbilityJobHandle);
        }
    }

    [BurstCompile]
    [WithAll(typeof(TriggerOnInAbilityRange))]
    [WithChangeFilter(typeof(ActivatedStateEntityOwner))]
    public partial struct SetupAbilityTrackingEntityInCastRange : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;
        void Execute([EntityIndexInQuery] int entityInQueryIndex, in ActivatedStateEntityOwner activatedStateEntityOwner)
        {
            this.Ecb.AddComponent<NeedToTrackingTargetInCastRange>(entityInQueryIndex, activatedStateEntityOwner.Value);
            this.Ecb.AddBuffer<EntityInAbilityRangeElement>(entityInQueryIndex, activatedStateEntityOwner.Value);
        }
    }

    [BurstCompile]
    public partial struct WaitOnTargetEntityInCastRangeJob : IJobEntity
    {
        public            EntityCommandBuffer.ParallelWriter       Ecb;
        [ReadOnly] public ComponentLookup<TriggerOnInAbilityRange> TriggerOnInAbilityRangeLookup;
        [ReadOnly] public BufferLookup<LinkedEntityGroup>          LinkedEntityLookup;
        void Execute([EntityIndexInQuery] int entityInQueryIndex, in OnInAbilityRange onInAbilityRange)
        {
            // Debug.Log($"WaitOnTargetEntityInCastRangeJob onInAbilityRange.TargetEntity.Index = {onInAbilityRange.TargetEntity.Index}");

            if (this.LinkedEntityLookup.TryGetBuffer(onInAbilityRange.ActivatedStateAbilityEntity, out var linkedEntityGroups))
            {
                foreach (var linkedEntity in linkedEntityGroups)
                {
                    if (this.TriggerOnInAbilityRangeLookup.HasComponent(linkedEntity.Value))
                    {
                        // Debug.Log($"WaitOnTargetEntityInCastRangeJob  {linkedEntity.Value.Index}");
                        //set target entity 
                        this.Ecb.AppendToBuffer(entityInQueryIndex, linkedEntity.Value, new TargetableElement() { Value = onInAbilityRange.TargetEntity });

                        //mark this condition was done
                        this.Ecb.MarkTriggerConditionComplete<TriggerOnInAbilityRange>(linkedEntity.Value, entityInQueryIndex);
                    }
                }
            }
        }
    }
}