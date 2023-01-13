namespace GASCore.Systems.StatSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.StatSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityLogicEffectGroup))]
    [UpdateAfter(typeof(ApplyPeriodEffectPolicySystem))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct ApplyStatModifierSystem : ISystem
    {
        private EntityQuery                          instantEffectEntityQuery;
        private EntityQuery                          tempEffectEntityQuery;
        private BufferLookup<ModifierAggregatorData> modifierAggregatorLookup;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            this.modifierAggregatorLookup = state.GetBufferLookup<ModifierAggregatorData>(true);
            using var queryBuilder = new EntityQueryBuilder(Allocator.Temp).WithAll<ModifierAggregatorData>().WithAny<InstantEffect, PeriodEffectInstanceTag>();
            this.instantEffectEntityQuery = state.GetEntityQuery(queryBuilder);

            queryBuilder.Reset();
            queryBuilder.WithAll<ModifierAggregatorData>().WithAny<DurationEffect, InfiniteEffect>().WithNone<PeriodEffect>();
            this.tempEffectEntityQuery = state.GetEntityQuery(queryBuilder);
            this.tempEffectEntityQuery.SetChangedVersionFilter(ComponentType.ReadOnly<ModifierAggregatorData>());
        }
        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            this.modifierAggregatorLookup.Update(ref state);

            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            var applyInstantEffectJob = new ApplyStatModifierJob()
            {
                Ecb                      = ecb,
                ModifierAggregatorLookup = this.modifierAggregatorLookup,
                IsChangeBaseValue        = true
            }.ScheduleParallel(instantEffectEntityQuery, state.Dependency);

            var applyTempEffectJob = new ApplyStatModifierJob()
            {
                Ecb                      = ecb,
                ModifierAggregatorLookup = this.modifierAggregatorLookup,
                IsChangeBaseValue        = false
            }.ScheduleParallel(tempEffectEntityQuery, applyInstantEffectJob);
            ;

            state.Dependency = new CalculateStatValueJob()
            {
                Ecb = ecb
            }.ScheduleParallel(applyTempEffectJob);
        }
    }

    [BurstCompile]
    public partial struct ApplyStatModifierJob : IJobEntity
    {
        public            EntityCommandBuffer.ParallelWriter   Ecb;
        public            bool                                 IsChangeBaseValue;
        [ReadOnly] public BufferLookup<ModifierAggregatorData> ModifierAggregatorLookup;
        void Execute([EntityInQueryIndex] int entityInQueryIndex, in AffectedTargetComponent affectedTarget, in DynamicBuffer<ModifierAggregatorData> modifierAggregatorBuffer)
        {
            if (!ModifierAggregatorLookup.HasBuffer(affectedTarget.Value)) Ecb.AddBuffer<ModifierAggregatorData>(entityInQueryIndex, affectedTarget.Value);

            foreach (var aggregator in modifierAggregatorBuffer)
            {
                var modifierAggregatorData = aggregator;
                modifierAggregatorData.IsChangeBaseValue = IsChangeBaseValue;
                Ecb.AppendToBuffer(entityInQueryIndex, affectedTarget.Value, modifierAggregatorData);
            }
        }
    }

    [BurstCompile]
    public partial struct CalculateStatValueJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;
        void Execute([EntityInQueryIndex] int entityInQueryIndex, ref StatAspect statAspect, ref DynamicBuffer<ModifierAggregatorData> modifierAggregatorBuffer)
        {
            if (modifierAggregatorBuffer.Length <= 0) return;
            var changedStats = new NativeHashSet<FixedString64Bytes>(statAspect.GetStatCount(), Allocator.Temp);
            // Apply stat modifier for base value first
            for (var index = 0; index < modifierAggregatorBuffer.Length; index++)
            {
                var aggregator = modifierAggregatorBuffer[index];
                if (!aggregator.IsChangeBaseValue) continue;
                if (statAspect.SetBaseValue(aggregator.TargetStat, statAspect.CalculateStatValue(aggregator))) changedStats.Add(aggregator.TargetStat);
            }

            // Apply stat modifier for added value
            var statNameToAddedValue = new NativeHashMap<FixedString64Bytes, float>(statAspect.GetStatCount(), Allocator.TempJob);
            foreach (var aggregator in modifierAggregatorBuffer)
            {
                if (aggregator.IsChangeBaseValue) continue;
                if (!statNameToAddedValue.TryGetValue(aggregator.TargetStat, out var addedValue))
                {
                    addedValue = 0;
                }

                statNameToAddedValue[aggregator.TargetStat] = addedValue + statAspect.CalculateStatValue(aggregator) - statAspect.GetBaseValue(aggregator.TargetStat);
            }

            foreach (var addedValue in statNameToAddedValue)
            {
                if (statAspect.SetAddedValue(addedValue.Key, addedValue.Value))
                {
                    changedStats.Add(addedValue.Key);
                }
            }

            //notify stat change
            foreach (var statName in changedStats)
            {
                statAspect.NotifyStatChange(Ecb, entityInQueryIndex, statName);
            }

            changedStats.Dispose();
            statNameToAddedValue.Dispose();
            modifierAggregatorBuffer.Clear();
        }
    }
}