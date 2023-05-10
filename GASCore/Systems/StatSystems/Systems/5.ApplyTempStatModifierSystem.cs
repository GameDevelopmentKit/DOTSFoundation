namespace GASCore.Systems.StatSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using GASCore.Systems.StatSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityLogicEffectGroup))]
    [UpdateAfter(typeof(ApplyInstantStatModifierSystem))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct ApplyTempStatModifierSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            new AddNewTempStatModifierToAffectedTargetJob()
            {
                Ecb                   = ecb,
                LinkedEntityLookup    = SystemAPI.GetBufferLookup<LinkedEntityGroup>(true),
                PeriodComponentLookup = SystemAPI.GetComponentLookup<PeriodEffect>(true)
            }.ScheduleParallel();

            new RemoveTempStatModifierJob() { Ecb = ecb }.ScheduleParallel();

            new CalculateAddedStatValueJob()
            {
                ModifierAggregatorDataLookup = SystemAPI.GetBufferLookup<ModifierAggregatorData>(true)
            }.ScheduleParallel();
        }
    }

    public struct OnUpdateTempStatModifierTag : IComponentData, IEnableableComponent { }

    public struct TempStatModifierCleanupComponent : ICleanupComponentData
    {
        public Entity AffectedTarget;
    }


    [BurstCompile]
    [WithAny(typeof(InfiniteEffect), typeof(DurationEffect))]
    [WithNone(typeof(IgnoreCleanupTag), typeof(Duration))]
    public partial struct AddNewTempStatModifierToAffectedTargetJob : IJobEntity
    {
        public            EntityCommandBuffer.ParallelWriter Ecb;
        [ReadOnly] public BufferLookup<LinkedEntityGroup>    LinkedEntityLookup;
        [ReadOnly] public ComponentLookup<PeriodEffect>      PeriodComponentLookup;

        void Execute(Entity statModifierEntity, [EntityIndexInQuery] int entityInQueryIndex, in AffectedTargetComponent affectedTarget)
        {
            if (!this.LinkedEntityLookup.HasBuffer(affectedTarget)) this.Ecb.AddBuffer<LinkedEntityGroup>(entityInQueryIndex, affectedTarget);
            this.Ecb.AppendToBuffer(entityInQueryIndex, affectedTarget, new LinkedEntityGroup() { Value = statModifierEntity });

            if (this.PeriodComponentLookup.HasComponent(statModifierEntity)) return;
            this.Ecb.SetComponentEnabled<OnUpdateTempStatModifierTag>(entityInQueryIndex, affectedTarget, true);
            this.Ecb.AddComponent(entityInQueryIndex, statModifierEntity, new TempStatModifierCleanupComponent() { AffectedTarget = affectedTarget.Value });
        }
    }

    [BurstCompile]
    [WithAll(typeof(TempStatModifierCleanupComponent))]
    [WithNone(typeof(ModifierAggregatorData))]
    public partial struct RemoveTempStatModifierJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;
        void Execute(Entity statModifierEntity, [EntityIndexInQuery] int entityInQueryIndex, in TempStatModifierCleanupComponent tempStatModifierCleanupComponent)
        {
            this.Ecb.SetComponentEnabled<OnUpdateTempStatModifierTag>(entityInQueryIndex, tempStatModifierCleanupComponent.AffectedTarget, true);
            this.Ecb.RemoveComponent<TempStatModifierCleanupComponent>(entityInQueryIndex, statModifierEntity);
        }
    }

    [BurstCompile]
    [WithAll(typeof(OnUpdateTempStatModifierTag))]
    public partial struct CalculateAddedStatValueJob : IJobEntity
    {
        [ReadOnly] public BufferLookup<ModifierAggregatorData> ModifierAggregatorDataLookup;
        void Execute(ref StatAspect statAspect, in DynamicBuffer<LinkedEntityGroup> linkedEntityGroups)
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
        }
    }
}