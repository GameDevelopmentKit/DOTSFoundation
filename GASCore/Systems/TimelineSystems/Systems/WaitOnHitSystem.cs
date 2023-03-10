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
    using Unity.Jobs;
    using Unity.Physics.Stateful;

    [UpdateInGroup(typeof(AbilityTimelineGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct WaitOnHitSystem : ISystem
    {
        private EntityQuery abilityTimelineTriggerOnHitQuery;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            using var queryBuilder = new EntityQueryBuilder(Allocator.Temp).WithAll<TriggerOnHit, ActivatedStateEntityOwner>();
            this.abilityTimelineTriggerOnHitQuery = state.GetEntityQuery(queryBuilder);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var triggerOnHitEntities = this.abilityTimelineTriggerOnHitQuery.ToEntityListAsync(state.WorldUpdateAllocator, state.Dependency, out var getEntitiesJob);
            var triggerOnHitComponents =
                this.abilityTimelineTriggerOnHitQuery.ToComponentDataListAsync<TriggerOnHit>(state.WorldUpdateAllocator, state.Dependency, out var getTriggerOnHitComponentJob);
            var activatedStateOwnerComponents =
                this.abilityTimelineTriggerOnHitQuery.ToComponentDataListAsync<ActivatedStateEntityOwner>(state.WorldUpdateAllocator, state.Dependency, out var getActivatedStateOwnerComponentJob);
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            var listenOnHitEventJob = new ListenOnHitEventJob
            {
                Ecb                           = ecb,
                TriggerOnHitEntities          = triggerOnHitEntities,
                TriggerOnHitComponents        = triggerOnHitComponents,
                ActivatedStateOwnerComponents = activatedStateOwnerComponents,
            };

            state.Dependency = listenOnHitEventJob.ScheduleParallel(JobHandle.CombineDependencies(getEntitiesJob, getTriggerOnHitComponentJob, getActivatedStateOwnerComponentJob));
        }
    }

    [BurstCompile]
    [WithChangeFilter(typeof(StatefulTriggerEvent))]
    public partial struct ListenOnHitEventJob : IJobEntity
    {
        public            EntityCommandBuffer.ParallelWriter    Ecb;
        [ReadOnly] public NativeList<Entity>                    TriggerOnHitEntities;
        [ReadOnly] public NativeList<TriggerOnHit>              TriggerOnHitComponents;
        [ReadOnly] public NativeList<ActivatedStateEntityOwner> ActivatedStateOwnerComponents;

        void Execute(Entity abilityActionEntity, [EntityIndexInQuery] int entityInQueryIndex, in AbilityEffectId effectId, in DynamicBuffer<StatefulTriggerEvent> triggerEventBuffer, in ActivatedStateEntityOwner activatedStateEntityOwner)
        {
            if (triggerEventBuffer.IsEmpty) return;
            for (var index = 0; index < this.TriggerOnHitEntities.Length; index++)
            {
                var triggerOnHitEntity    = this.TriggerOnHitEntities[index];
                var triggerOnHitComponent = this.TriggerOnHitComponents[index];
                if (!this.ActivatedStateOwnerComponents[index].Value.Equals(activatedStateEntityOwner.Value) ||
                    !triggerOnHitComponent.FromAbilityEffectId.Equals(effectId.Value)) continue;
                var isHit = false;
                //set target entity for activated state entity for common access
                foreach (var triggerEvent in triggerEventBuffer)
                {
                    if ((triggerOnHitComponent.StateType & triggerEvent.State) != triggerEvent.State) continue;
                    isHit = true;
                    var otherEntity = triggerEvent.GetOtherEntity(abilityActionEntity);
                    this.Ecb.AppendToBuffer(entityInQueryIndex, triggerOnHitEntity, new TargetableElement() { Value = otherEntity });
                }

                if (isHit)
                {
                    //mark this condition was done
                    this.Ecb.MarkTriggerConditionComplete<TriggerOnHit>(triggerOnHitEntity, entityInQueryIndex);
                    if (triggerOnHitComponent.IsDestroyAbilityEffectOnHit)
                        this.Ecb.AddComponent<ForceCleanupTag>(entityInQueryIndex, abilityActionEntity);
                }

                break;
            }
        }
    }
}