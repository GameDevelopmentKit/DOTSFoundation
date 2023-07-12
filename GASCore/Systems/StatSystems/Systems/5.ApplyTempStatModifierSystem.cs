namespace GASCore.Systems.StatSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.StatSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;

    public struct OnUpdateTempStatModifierTag : IComponentData, IEnableableComponent
    {
    }

    public struct MarkSetupTempEffect : IComponentData
    {
    }

    public struct TempStatModifierCleanupComponent : ICleanupComponentData
    {
        public Entity AffectedTarget;
    }

    [UpdateInGroup(typeof(AbilityLogicEffectGroup))]
    [UpdateAfter(typeof(ApplyInstantStatModifierSystem))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct SetupTempStatModifierSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { state.RequireForUpdate(SystemAPI.QueryBuilder().WithAny<InfiniteEffect,DurationEffect>().WithNone<MarkSetupTempEffect>().Build()); }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            new SetupTempStatModifierJob()
            {
                Ecb                   = ecb,
                LinkedEntityLookup    = SystemAPI.GetBufferLookup<LinkedEntityGroup>(true),
                PeriodComponentLookup = SystemAPI.GetComponentLookup<PeriodEffect>(true)
            }.ScheduleParallel();
        }

        [BurstCompile]
        [WithAny(typeof(InfiniteEffect), typeof(DurationEffect))]
        [WithNone(typeof(MarkSetupTempEffect))]
        public partial struct SetupTempStatModifierJob : IJobEntity
        {
            public            EntityCommandBuffer.ParallelWriter Ecb;
            [ReadOnly] public BufferLookup<LinkedEntityGroup>    LinkedEntityLookup;
            [ReadOnly] public ComponentLookup<PeriodEffect>      PeriodComponentLookup;

            void Execute(Entity statModifierEntity, [EntityIndexInQuery] int entityInQueryIndex, in AffectedTargetComponent affectedTarget)
            {
                this.Ecb.AddComponent<MarkSetupTempEffect>(entityInQueryIndex, statModifierEntity);
                
                if (!this.LinkedEntityLookup.HasBuffer(affectedTarget)) this.Ecb.AddBuffer<LinkedEntityGroup>(entityInQueryIndex, affectedTarget);
                this.Ecb.AppendToBuffer(entityInQueryIndex, affectedTarget, new LinkedEntityGroup() { Value = statModifierEntity });

                if (this.PeriodComponentLookup.HasComponent(statModifierEntity)) return;
                this.Ecb.SetComponentEnabled<OnUpdateTempStatModifierTag>(entityInQueryIndex, affectedTarget, true);
                this.Ecb.AddComponent(entityInQueryIndex, statModifierEntity, new TempStatModifierCleanupComponent() { AffectedTarget = affectedTarget.Value });
            }
        }
    }

    [UpdateInGroup(typeof(AbilityLogicEffectGroup))]
    [UpdateAfter(typeof(ApplyInstantStatModifierSystem))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct RemoveTempStatModifierSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { state.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<TempStatModifierCleanupComponent>().WithNone<ModifierAggregatorData>().Build()); }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            new RemoveTempStatModifierJob()
            {
                Ecb                            = ecb,
                OnUpdateTempStatModifierLookup = SystemAPI.GetComponentLookup<OnUpdateTempStatModifierTag>()
            }.ScheduleParallel();
        }

        [BurstCompile]
        [WithAll(typeof(TempStatModifierCleanupComponent))]
        [WithNone(typeof(ModifierAggregatorData))]
        public partial struct RemoveTempStatModifierJob : IJobEntity
        {
            public            EntityCommandBuffer.ParallelWriter           Ecb;
            [ReadOnly] public ComponentLookup<OnUpdateTempStatModifierTag> OnUpdateTempStatModifierLookup;
            void Execute(Entity statModifierEntity, [EntityIndexInQuery] int entityInQueryIndex, in TempStatModifierCleanupComponent tempStatModifierCleanupComponent)
            {
                //todo need to double check this condition
                if (OnUpdateTempStatModifierLookup.HasComponent(tempStatModifierCleanupComponent.AffectedTarget))
                    this.Ecb.SetComponentEnabled<OnUpdateTempStatModifierTag>(entityInQueryIndex, tempStatModifierCleanupComponent.AffectedTarget, true);
                this.Ecb.RemoveComponent<TempStatModifierCleanupComponent>(entityInQueryIndex, statModifierEntity);
            }
        }
    }

    [UpdateInGroup(typeof(AbilityLogicEffectGroup))]
    [UpdateAfter(typeof(ApplyInstantStatModifierSystem))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct CalculateAddedStatValueSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new CalculateAddedStatValueJob()
            {
                ModifierAggregatorDataLookup = SystemAPI.GetBufferLookup<ModifierAggregatorData>(true)
            }.ScheduleParallel();
        }

        [BurstCompile]
        [WithAll(typeof(OnUpdateTempStatModifierTag))]
        public partial struct CalculateAddedStatValueJob : IJobEntity
        {
            [ReadOnly] public BufferLookup<ModifierAggregatorData> ModifierAggregatorDataLookup;
            void Execute(StatAspect statAspect, in DynamicBuffer<LinkedEntityGroup> linkedEntityGroups, EnabledRefRW<OnUpdateTempStatModifierTag> onUpdateEnableState)
            {
                statAspect.ResetAllAddedValue();

                foreach (var linkedEntity in linkedEntityGroups)
                {
                    if (!this.ModifierAggregatorDataLookup.TryGetBuffer(linkedEntity.Value, out var modifierAggregatorBuffer)) continue;
                    foreach (var aggregator in modifierAggregatorBuffer)
                    {
                        var curStatData = statAspect.GetStatData(aggregator.TargetStat);
                        if (!curStatData.HasValue) continue;
                        var newAddedValue = curStatData.Value.AddedValue + statAspect.CalculateStatValue(aggregator) - curStatData.Value.BaseValue;
                        statAspect.SetAddedValue(aggregator.TargetStat, newAddedValue);
                    }
                }

                onUpdateEnableState.ValueRW = true;
            }
        }
    }
}