namespace GASCore.Systems.TimelineSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Services;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.TimelineSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using UnityEngine;

    //This system should be update in AbilityVisualEffectGroup, to listen the OnStatChange notify before it's destroyed
    [UpdateInGroup(typeof(AbilityVisualEffectGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct WaitOnStatChangeSystem : ISystem
    {
        private EntityQuery                           triggerOnStatChangeQuery;
        private ComponentLookup<TriggerOnStatChanged> triggerOnStatChangeComponentLookup;
        private ComponentLookup<CasterComponent>      casterLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            using var queryBuilder = new EntityQueryBuilder(Allocator.Temp).WithAll<TriggerOnStatChanged>();
            this.triggerOnStatChangeQuery = state.GetEntityQuery(queryBuilder);

            this.triggerOnStatChangeComponentLookup = state.GetComponentLookup<TriggerOnStatChanged>(true);
            this.casterLookup                       = state.GetComponentLookup<CasterComponent>(true);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            this.triggerOnStatChangeComponentLookup.Update(ref state);
            this.casterLookup.Update(ref state);

            var triggerOnHits = this.triggerOnStatChangeQuery.ToEntityListAsync(state.WorldUpdateAllocator, out var getTriggerOnHitHandle);

            var ecbSingleton = SystemAPI.GetSingleton<AbilityPresentEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            var listenOnHitEventJob = new ListenOnStatChangedJob()
            {
                Ecb                                = ecb,
                TriggerOnStatChangeEntities        = triggerOnHits,
                TriggerOnStatChangeComponentLookup = this.triggerOnStatChangeComponentLookup,
                CasterLookup                       = this.casterLookup,
            };

            state.Dependency = listenOnHitEventJob.ScheduleParallel(getTriggerOnHitHandle);
        }
    }


    [BurstCompile]
    public partial struct ListenOnStatChangedJob : IJobEntity
    {
        public            EntityCommandBuffer.ParallelWriter    Ecb;
        [ReadOnly] public NativeList<Entity>                    TriggerOnStatChangeEntities;
        [ReadOnly] public ComponentLookup<TriggerOnStatChanged> TriggerOnStatChangeComponentLookup;
        [ReadOnly] public ComponentLookup<CasterComponent>      CasterLookup;

        void Execute(Entity sourceStatChangeEntity, [EntityIndexInQuery] int entityInQueryIndex, in DynamicBuffer<OnStatChange> statChangeEventBuffer)
        {
            foreach (var triggerEntity in this.TriggerOnStatChangeEntities)
            {
                if (!this.CasterLookup[triggerEntity].Value.Equals(sourceStatChangeEntity)) continue; // wrong entity

                var triggerCondition = this.TriggerOnStatChangeComponentLookup[triggerEntity];

                foreach (var onStatChange in statChangeEventBuffer)
                {
                    if (!triggerCondition.StatName.Equals(onStatChange.ChangedStat.StatName)) continue; // wrong stat name 

                    var currentValue = triggerCondition.Percent
                        ? onStatChange.ChangedStat.CurrentValue / onStatChange.ChangedStat.OriginValue
                        : onStatChange.ChangedStat.CurrentValue;
                    if (triggerCondition.Above && currentValue >= triggerCondition.Value
                        || !triggerCondition.Above && currentValue <= triggerCondition.Value)
                    {
                        // Debug.Log($"ListenOnStatChangedJob from stat {event_.ChangedStat.StatName}");
                        // mark this condition was done
                        this.Ecb.MarkTriggerConditionComplete<TriggerOnStatChanged>(triggerEntity, entityInQueryIndex);
                    }
                }
            }
        }
    }
}