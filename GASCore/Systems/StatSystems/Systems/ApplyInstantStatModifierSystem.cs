namespace GASCore.Systems.StatSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.StatSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using UnityEngine;

    [UpdateInGroup(typeof(AbilityLogicEffectGroup))]
    [UpdateAfter(typeof(ApplyPeriodEffectPolicySystem))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct ApplyInstantStatModifierSystem : ISystem
    {
        private EntityQuery                       instantEffectEntityQuery;
        private ComponentLookup<StatModifierData> statModifierDataLookup;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            using var queryBuilder = new EntityQueryBuilder(Allocator.Temp).WithAll<StatModifierEntityElement>().WithAny<InstantEffect, PeriodEffectInstanceTag>();
            this.instantEffectEntityQuery = state.GetEntityQuery(queryBuilder);
            // this.instantEffectEntityQuery.SetChangedVersionFilter(typeof(StatModifierEntityElement));
            this.statModifierDataLookup = state.GetComponentLookup<StatModifierData>(true);
        }
        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            this.statModifierDataLookup.Update(ref state);
            var modifierAggregatorContainer = new ModifierAggregatorContainer(instantEffectEntityQuery.CalculateEntityCount());

            var aggregateStatModifierJob = new AggregateStatModifierJob()
            {
                StatModifierDataLookup      = this.statModifierDataLookup,
                ModifierAggregatorContainer = modifierAggregatorContainer,
            };
            var aggregateStatModifierJobHandle = aggregateStatModifierJob.Schedule(instantEffectEntityQuery, state.Dependency);

            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            var applyStatModifierJobHandle = new ApplyInstantEffectJob()
            {
                Ecb                              = ecb,
                EntityToMultiModifierAggregators = modifierAggregatorContainer.EntityAffectedTargetToMultiModifierAggregators,
            }.ScheduleParallel(aggregateStatModifierJobHandle);
            state.Dependency = applyStatModifierJobHandle;
            modifierAggregatorContainer.Dispose(state.Dependency);
        }
    }

    [BurstCompile]
    public partial struct ApplyInstantEffectJob : IJobEntity
    {
        public            EntityCommandBuffer.ParallelWriter                 Ecb;
        [ReadOnly] public NativeMultiHashMap<Entity, ModifierDataAggregator> EntityToMultiModifierAggregators;
        void Execute(Entity entity, [EntityInQueryIndex] int entityInQueryIndex, ref StatAspect statAspect)
        {
            //todo try to reduce call Execute times
            if (this.EntityToMultiModifierAggregators.IsEmpty || this.EntityToMultiModifierAggregators.CountValuesForKey(entity) < 0) return;

            var listModifierAggregators = this.EntityToMultiModifierAggregators.GetValuesForKey(entity);
            // Apply stat modifier to affect targets 
            foreach (var aggregator in listModifierAggregators)
            {
                statAspect.SetBaseValueAndNotify(this.Ecb, entityInQueryIndex, aggregator.TargetStat, statAspect.CalculateStatValue(aggregator));
            }
        }
    }
}