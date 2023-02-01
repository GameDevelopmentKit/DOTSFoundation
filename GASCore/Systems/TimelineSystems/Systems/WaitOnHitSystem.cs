namespace GASCore.Systems.TimelineSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Services;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using GASCore.Systems.TargetDetectionSystems.Components;
    using GASCore.Systems.TimelineSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Physics.Stateful;
    using UnityEngine;

    [UpdateInGroup(typeof(AbilityTimelineGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct WaitOnHitSystem : ISystem
    {
        private EntityQuery                                abilityTimelineTriggerOnHitQuery;
        private ComponentLookup<TriggerOnHit>              triggerOnHitLookup;
        private ComponentLookup<ActivatedStateEntityOwner> ownerLookup;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            using var queryBuilder = new EntityQueryBuilder(Allocator.Temp).WithAll<TriggerOnHit>().WithNone<OnHitTargetElement>();
            this.abilityTimelineTriggerOnHitQuery = state.GetEntityQuery(queryBuilder);

            this.triggerOnHitLookup              = state.GetComponentLookup<TriggerOnHit>(true);
            this.ownerLookup                     = state.GetComponentLookup<ActivatedStateEntityOwner>(true);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            this.triggerOnHitLookup.Update(ref state);
            this.ownerLookup.Update(ref state);
            var triggerOnHits = this.abilityTimelineTriggerOnHitQuery.ToEntityArray(state.WorldUpdateAllocator);

            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            var listenOnHitEventJob = new ListenOnHitEventJob()
            {
                Ecb                             = ecb,
                TriggerOnHitEntities            = triggerOnHits,
                TriggerOnHitLookup              = this.triggerOnHitLookup,
                OwnerLookup                     = this.ownerLookup,
            };

            state.Dependency = listenOnHitEventJob.ScheduleParallel(state.Dependency);
        }
    }

    [BurstCompile]
    [WithChangeFilter(typeof(StatefulTriggerEvent))]
    public partial struct ListenOnHitEventJob : IJobEntity
    {
        public            EntityCommandBuffer.ParallelWriter         Ecb;
        [ReadOnly] public NativeArray<Entity>                        TriggerOnHitEntities;
        [ReadOnly] public ComponentLookup<TriggerOnHit>              TriggerOnHitLookup;
        [ReadOnly] public ComponentLookup<ActivatedStateEntityOwner> OwnerLookup;
        void Execute(Entity abilityActionEntity, [EntityInQueryIndex] int entityInQueryIndex, in AbilityEffectId effectId, in DynamicBuffer<StatefulTriggerEvent> triggerEventBuffer)
        {
            if (triggerEventBuffer.IsEmpty) return;
            foreach (var triggerOnHitEntity in this.TriggerOnHitEntities)
            {
                var triggerOnHit = this.TriggerOnHitLookup[triggerOnHitEntity];
                if (this.OwnerLookup[triggerOnHitEntity].Value.Equals(this.OwnerLookup[abilityActionEntity].Value) &&
                    triggerOnHit.FromAbilityEffectId.Equals(effectId.Value))
                {
                    // Debug.Log($"ListenOnHitEventJob - add hit target to {triggerOnHitEntity.Index}, effectId.Value = {effectId.Value}");

                    var isHit = false;
                    //set target entity for activated state entity for common access
                    for (var i = 0; i < triggerEventBuffer.Length; i++)
                    {
                        var triggerEvent = triggerEventBuffer[i];
                        if (triggerEvent.State == StatefulEventState.Enter)
                        {
                            isHit = true;
                            var otherEntity = triggerEvent.GetOtherEntity(abilityActionEntity);
                            this.Ecb.AppendToBuffer(entityInQueryIndex, triggerOnHitEntity, new TargetableElement() { Value = otherEntity });
                        }
                    }

                    if (isHit)
                    {
                        //mark this condition was done
                        this.Ecb.MarkTriggerConditionComplete<TriggerOnHit>(triggerOnHitEntity, entityInQueryIndex);
                        if (triggerOnHit.IsDestroyAbilityEffectOnHit)
                            this.Ecb.AddComponent<ForceCleanupTag>(entityInQueryIndex, abilityActionEntity);
                    }

                    break;
                }
            }
        }
    }
}