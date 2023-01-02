namespace GASCore.Systems.TimelineSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using GASCore.Systems.TimelineSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;
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
        private ComponentLookup<TriggerConditionCount>     triggerConditionComponentLookup;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            using var queryBuilder = new EntityQueryBuilder(Allocator.Temp).WithAll<TriggerOnHit>().WithNone<OnHitTargetElement>();
            this.abilityTimelineTriggerOnHitQuery = state.GetEntityQuery(queryBuilder);

            this.triggerOnHitLookup              = state.GetComponentLookup<TriggerOnHit>(true);
            this.ownerLookup                     = state.GetComponentLookup<ActivatedStateEntityOwner>(true);
            this.triggerConditionComponentLookup = state.GetComponentLookup<TriggerConditionCount>(true);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            this.triggerOnHitLookup.Update(ref state);
            this.ownerLookup.Update(ref state);
            this.triggerConditionComponentLookup.Update(ref state);
            var triggerOnHits = this.abilityTimelineTriggerOnHitQuery.ToEntityArray(state.WorldUpdateAllocator);

            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            var listenOnHitEventJob = new ListenOnHitEventJob()
            {
                Ecb                             = ecb,
                TriggerOnHitEntities            = triggerOnHits,
                TriggerOnHitLookup              = this.triggerOnHitLookup,
                OwnerLookup                     = this.ownerLookup,
                TriggerConditionComponentLookup = this.triggerConditionComponentLookup,
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
        [ReadOnly] public ComponentLookup<TriggerConditionCount>     TriggerConditionComponentLookup;
        void Execute(Entity abilityActionEntity, [EntityInQueryIndex] int entityInQueryIndex, in AbilityEffectId effectId, in DynamicBuffer<StatefulTriggerEvent> triggerEventBuffer)
        {
            if(triggerEventBuffer.IsEmpty) return;
            foreach (var triggerOnHitEntity in this.TriggerOnHitEntities)
            {
                if (this.OwnerLookup[triggerOnHitEntity].Value.Equals(this.OwnerLookup[abilityActionEntity].Value) &&
                    this.TriggerOnHitLookup[triggerOnHitEntity].FromAbilityEffectId.Equals(effectId.Value))
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
                            this.Ecb.AppendToBuffer(entityInQueryIndex, triggerOnHitEntity, new TargetElement() { Value = otherEntity });
                        }
                    }

                    if (isHit)
                    {
                        //mark this condition was done
                        this.Ecb.SetComponent(entityInQueryIndex, triggerOnHitEntity, new TriggerConditionCount() { Value = this.TriggerConditionComponentLookup[triggerOnHitEntity].Value - 1 });
                    }
                    
                    break;
                }
            }
        }
    }
}