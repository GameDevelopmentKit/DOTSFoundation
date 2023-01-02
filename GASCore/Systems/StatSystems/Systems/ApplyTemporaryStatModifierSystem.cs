namespace GASCore.Systems.StatSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.StatSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using UnityEngine;

    [UpdateInGroup(typeof(AbilityLogicEffectGroup),OrderLast = true)]
    [UpdateAfter(typeof(ApplyInstantStatModifierSystem))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct ApplyTemporaryStatModifierSystem : ISystem
    {
        private EntityQuery                       tempEffectEntityQuery;
        private ComponentLookup<StatModifierData> statModifierDataLookup;
        private int                               latestCount;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            using var queryBuilder = new EntityQueryBuilder(Allocator.Temp).WithAll<StatModifierEntityElement>().WithAny<DurationEffect, InfiniteEffect>().WithNone<PeriodEffect>();
            this.tempEffectEntityQuery  = state.GetEntityQuery(queryBuilder);
            this.statModifierDataLookup = state.GetComponentLookup<StatModifierData>(true);
        }
        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var calculateEntityCount = this.tempEffectEntityQuery.CalculateEntityCount();
            //Check list entity of  tempEffectEntityQuery change
            if (calculateEntityCount > this.latestCount)
            {
                this.latestCount = calculateEntityCount;
                this.statModifierDataLookup.Update(ref state);
                var modifierAggregatorContainer = new ModifierAggregatorContainer(calculateEntityCount);
                
                var aggregateStatModifierJob = new AggregateStatModifierJob()
                {
                    StatModifierDataLookup      = this.statModifierDataLookup,
                    ModifierAggregatorContainer = modifierAggregatorContainer,
                };
                var aggregateStatModifierJobHandle = aggregateStatModifierJob.Schedule(tempEffectEntityQuery, state.Dependency);

                var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
                var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
                var applyStatModifierJobHandle = new ApplyTemporaryEffectJob()
                {
                    Ecb                              = ecb,
                    EntityToMultiModifierAggregators = modifierAggregatorContainer.EntityAffectedTargetToMultiModifierAggregators,
                }.ScheduleParallel(aggregateStatModifierJobHandle);
                state.Dependency = applyStatModifierJobHandle;
                modifierAggregatorContainer.Dispose(state.Dependency);
            }
        }
    }

    [BurstCompile]
    public partial struct ApplyTemporaryEffectJob : IJobEntity
    {
        public            EntityCommandBuffer.ParallelWriter                 Ecb;
        [ReadOnly] public NativeMultiHashMap<Entity, ModifierDataAggregator> EntityToMultiModifierAggregators;
        void Execute(Entity entity, [EntityInQueryIndex] int entityInQueryIndex, ref StatAspect statAspect)
        {
            //todo try to reduce call Execute times
            if (this.EntityToMultiModifierAggregators.IsEmpty || this.EntityToMultiModifierAggregators.CountValuesForKey(entity) < 0) return;

            var listModifierAggregators = this.EntityToMultiModifierAggregators.GetValuesForKey(entity);
            Debug.Log($"ApplyInstantEffectJob Apply stat modifier to affect targets to {entity.Index}");
            // Apply stat modifier to affect targets 
            foreach (var aggregator in listModifierAggregators)
            {
                statAspect.SetCurrentValueAndNotify(this.Ecb, entityInQueryIndex, aggregator.TargetStat, statAspect.CalculateStatValue(aggregator));
            }
        }
    }
}