namespace GASCore.Systems.TimelineSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Services;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.TimelineSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;

    //This system should be update in AbilityVisualEffectGroup, to listen the OnStatChange notify before it's destroyed
    [UpdateInGroup(typeof(AbilityVisualEffectGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct WaitOnStatChangeSystem : ISystem
    {
        private EntityQuery triggerOnStatChangeQuery;
        private EntityQuery statChangeEntityQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            using var queryBuilder = new EntityQueryBuilder(Allocator.Temp).WithAll<TriggerOnStatChanged>();
            this.triggerOnStatChangeQuery = state.GetEntityQuery(queryBuilder);
            statChangeEntityQuery         = SystemAPI.QueryBuilder().WithAll<OnStatChangeTag, StatChangeElement>().Build();
            state.RequireForUpdate(statChangeEntityQuery);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (statChangeEntityQuery.IsEmpty) return;
            var triggerOnHits = this.triggerOnStatChangeQuery.ToEntityListAsync(state.WorldUpdateAllocator, out var getTriggerOnHitHandle);

            var ecbSingleton = SystemAPI.GetSingleton<BeginPresentationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            var listenOnHitEventJob = new ListenOnStatChangedJob()
            {
                Ecb                                = ecb,
                TriggerOnStatChangeEntities        = triggerOnHits,
                TriggerOnStatChangeComponentLookup = SystemAPI.GetComponentLookup<TriggerOnStatChanged>(true),
                CasterLookup                       = SystemAPI.GetComponentLookup<CasterComponent>(true),
            };

            state.Dependency = listenOnHitEventJob.ScheduleParallel(getTriggerOnHitHandle);
        }
    }


    [BurstCompile]
    [WithAll(typeof(OnStatChangeTag))]
    public partial struct ListenOnStatChangedJob : IJobEntity
    {
        public            EntityCommandBuffer.ParallelWriter    Ecb;
        [ReadOnly] public NativeList<Entity>                    TriggerOnStatChangeEntities;
        [ReadOnly] public ComponentLookup<TriggerOnStatChanged> TriggerOnStatChangeComponentLookup;
        [ReadOnly] public ComponentLookup<CasterComponent>      CasterLookup;

        void Execute(Entity sourceStatChangeEntity, [EntityIndexInQuery] int entityInQueryIndex, in DynamicBuffer<StatChangeElement> statChangeEventBuffer)
        {
            foreach (var triggerEntity in this.TriggerOnStatChangeEntities)
            {
                if (!this.CasterLookup[triggerEntity].Value.Equals(sourceStatChangeEntity)) continue; // wrong entity

                var triggerCondition = this.TriggerOnStatChangeComponentLookup[triggerEntity];

                foreach (var onStatChange in statChangeEventBuffer)
                {
                    if (!triggerCondition.AnyStat && !triggerCondition.StatName.Equals(onStatChange.Value.StatName)) continue; // wrong stat name 
                    var currentValue = triggerCondition.Percent
                        ? onStatChange.Value.CurrentValue / onStatChange.Value.OriginValue
                        : onStatChange.Value.CurrentValue;
                    if (triggerCondition.Above && currentValue >= triggerCondition.Value
                        || !triggerCondition.Above && currentValue <= triggerCondition.Value)
                    {
                        // Debug.Log($"ListenOnStatChangedJob from stat {event_.ChangedStat.StatName}");
                        // mark this condition was done
                        this.Ecb.MarkTriggerConditionComplete<TriggerOnStatChanged>(triggerEntity, entityInQueryIndex);
                        break;
                    }
                }
            }
        }
    }
}