namespace GASCore.Systems.StatSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.StatSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityLogicEffectGroup))]
    [UpdateAfter(typeof(CalculateStatModifierMagnitudeSystem))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct AggregateStatModifierSystem : ISystem
    {
        private ComponentLookup<StatModifierData> statModifierDataLookup;
        [BurstCompile]
        public void OnCreate(ref SystemState state) { this.statModifierDataLookup = state.GetComponentLookup<StatModifierData>(true); }
        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            this.statModifierDataLookup.Update(ref state);

            EntityCommandBuffer                ecb         = new EntityCommandBuffer(Allocator.TempJob);
            EntityCommandBuffer.ParallelWriter ecbParallel = ecb.AsParallelWriter();
            var aggregateJob = new AggregateStatModifierSystemJob()
            {
                EcbParallel            = ecbParallel,
                StatModifierDataLookup = this.statModifierDataLookup,
            }.ScheduleParallel(state.Dependency);

            aggregateJob.Complete();
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }


    /// <summary>
    /// Aggregate stat modifier for each effect entity
    /// </summary>
    [BurstCompile]
    [WithNone(typeof(ModifierAggregatorData))]
    public partial struct AggregateStatModifierSystemJob : IJobEntity
    {
        public            EntityCommandBuffer.ParallelWriter EcbParallel;
        [ReadOnly] public ComponentLookup<StatModifierData>  StatModifierDataLookup;
        void Execute(Entity effectEntity, [EntityInQueryIndex] int entityInQueryIndex, in DynamicBuffer<StatModifierEntityElement> statModifierEntityElementBuffers)
        {
            //aggregate all modifier
            var statNameToModifierAggregators = new NativeHashMap<FixedString64Bytes, ModifierAggregatorData>(statModifierEntityElementBuffers.Length, Allocator.Temp);
            foreach (var statModifierElementEntity in statModifierEntityElementBuffers)
            {
                var statModifierData = this.StatModifierDataLookup[statModifierElementEntity.Value];

                if (!statNameToModifierAggregators.TryGetValue(statModifierData.TargetStat, out var dataAggregator))
                {
                    dataAggregator = new ModifierAggregatorData()
                    {
                        TargetStat = statModifierData.TargetStat,
                        Add        = 0,
                        Multiply   = 1,
                        Division   = 1
                    };
                }

                switch (statModifierData.ModifierOperator)
                {
                    case ModifierOperatorType.Add:
                        dataAggregator.Add += statModifierData.ModifierMagnitude;
                        break;
                    case ModifierOperatorType.Multiply:
                        dataAggregator.Multiply *= statModifierData.ModifierMagnitude;
                        break;
                    case ModifierOperatorType.Division:
                        dataAggregator.Division *= statModifierData.ModifierMagnitude;
                        break;
                    case ModifierOperatorType.Override:
                        dataAggregator.Override = statModifierData.ModifierMagnitude;
                        break;
                }

                statNameToModifierAggregators[statModifierData.TargetStat] = dataAggregator;
            }

            // cache aggregated modifier to buffer
            var modBuffer = this.EcbParallel.AddBuffer<ModifierAggregatorData>(entityInQueryIndex, effectEntity);
            foreach (var modifierAggregatorData in statNameToModifierAggregators)
            {
                modBuffer.Add(modifierAggregatorData.Value);
            }
        }
    }
}