namespace GASCore.Systems.TargetDetectionSystems.Systems.Filters
{
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using GASCore.Systems.TargetDetectionSystems.Components;
    using GASCore.Systems.TargetDetectionSystems.Components.Filters;
    using GASCore.Systems.VisualEffectSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;
    using Unity.Physics.Stateful;
    using Unity.Transforms;

    [UpdateInGroup(typeof(FindTargetGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct FilterOnHitSystem : ISystem
    {
        private EntityQuery onCollisionEntityQuery;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            this.onCollisionEntityQuery = SystemAPI.QueryBuilder().WithAll<OnCollisionTag, AbilityEffectId, ActivatedStateEntityOwner, StatefulTriggerEvent, LocalToWorld>().Build();
            this.onCollisionEntityQuery.SetChangedVersionFilter(ComponentType.ReadOnly<StatefulTriggerEvent>());
            state.RequireForUpdate(this.onCollisionEntityQuery);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (this.onCollisionEntityQuery.IsEmpty) return;
            var triggerOnHitEntities = this.onCollisionEntityQuery.ToEntityArray(state.WorldUpdateAllocator);
            new FilterOnHitJob
            {
                OnCollisionEntities           = triggerOnHitEntities,
                AbilityEffectIdComponents     = SystemAPI.GetComponentLookup<AbilityEffectId>(true),
                ActivatedStateOwnerComponents = SystemAPI.GetComponentLookup<ActivatedStateEntityOwner>(true),
                StatefulTriggerEventLookup    = SystemAPI.GetBufferLookup<StatefulTriggerEvent>(true),
                UntargetableLookup            = SystemAPI.GetComponentLookup<UntargetableTag>(true),
                CasterLookup                  = SystemAPI.GetComponentLookup<CasterComponent>(true),
            }.ScheduleParallel();
        }
    }

    [WithAll(typeof(FindTargetComponent))]
    [BurstCompile]
    public partial struct FilterOnHitJob : IJobEntity
    {
        [ReadOnly] public NativeArray<Entity>                        OnCollisionEntities;
        [ReadOnly] public ComponentLookup<AbilityEffectId>           AbilityEffectIdComponents;
        [ReadOnly] public ComponentLookup<ActivatedStateEntityOwner> ActivatedStateOwnerComponents;
        [ReadOnly] public ComponentLookup<CasterComponent>           CasterLookup;
        [ReadOnly] public BufferLookup<StatefulTriggerEvent>         StatefulTriggerEventLookup;

        //TODO try to find better way later
        [ReadOnly] public ComponentLookup<UntargetableTag> UntargetableLookup;

        private void Execute(ref DynamicBuffer<TargetableElement> targetables, ref DynamicBuffer<CacheTriggerEventElement> cacheTriggerEventBuffer, in FilterOnHit filterOnHit,
            in ActivatedStateEntityOwner activatedStateEntityOwner, in CasterComponent caster)
        {
            targetables.Clear();
            for (var index = 0; index < this.OnCollisionEntities.Length; index++)
            {
                var collisionEntity = this.OnCollisionEntities[index];
                // from same caster
                if (!this.CasterLookup[collisionEntity].Value.Equals(caster)) continue;
                // collision from same activated ability
                if (filterOnHit.IsLocal && (!this.ActivatedStateOwnerComponents[collisionEntity].Value.Equals(activatedStateEntityOwner.Value) ||
                                            !this.AbilityEffectIdComponents[collisionEntity].Value.Equals(filterOnHit.FromAbilityEffectId))) continue;

                //set target entity for activated state entity for common access
                foreach (var triggerEvent in StatefulTriggerEventLookup[collisionEntity])
                {
                    if ((filterOnHit.StateType & triggerEvent.State) != triggerEvent.State) continue;
                    var otherEntity = triggerEvent.GetOtherEntity(collisionEntity);
                    if (UntargetableLookup.HasComponent(otherEntity) && UntargetableLookup.IsComponentEnabled(otherEntity)) continue;
                    targetables.Add(new TargetableElement() { Value                           = otherEntity });
                    cacheTriggerEventBuffer.Add(new CacheTriggerEventElement() { SourceEntity = collisionEntity, OtherEntity = otherEntity });
                }
            }
        }
    }
}