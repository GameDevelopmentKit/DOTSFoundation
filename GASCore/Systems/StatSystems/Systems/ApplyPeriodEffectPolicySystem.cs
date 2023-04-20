namespace GASCore.Systems.StatSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using GASCore.Systems.StatSystems.Components;
    using Unity.Burst;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityLogicEffectGroup))]
    [UpdateAfter(typeof(ApplyTempEffectPolicySystem))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct ApplyPeriodEffectPolicySystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { }
        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var endEcbSingleton    = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var endEcb                = endEcbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            var currentElapsedTime = SystemAPI.Time.ElapsedTime;
            new ApplyPeriodEffectPolicyJob()
            {
                Ecb                = endEcb,
                CurrentElapsedTime = currentElapsedTime
            }.ScheduleParallel();
            
            var beginEcbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var beginEcb          = beginEcbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
           state.Dependency = new CreatePeriodInstanceEffectJob()
            {
                Ecb                = beginEcb,
            }.ScheduleParallel(state.Dependency);
        }
    }

    /// <summary>
    /// Add end time for period effect
    /// </summary>
    [BurstCompile]
    [WithNone(typeof(EndTimeComponent))]
    public partial struct ApplyPeriodEffectPolicyJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;
        public double                             CurrentElapsedTime;

        void Execute(Entity statModifierEntity, [EntityIndexInQuery] int entityInQueryIndex, in PeriodEffect periodEffect)
        {
            //wait period in second
            this.Ecb.AddComponent(entityInQueryIndex, statModifierEntity, new EndTimeComponent() { Value = this.CurrentElapsedTime + periodEffect.Value });
            this.Ecb.SetComponentEnabled<EndTimeComponent>(entityInQueryIndex, statModifierEntity, true);
        }
    }
    
    
    /// <summary>
    /// Instantiate a period instance effect and wait after 'PeriodEffect.Value' second to create a new one, until this stat modifier is removed
    /// </summary>
    [BurstCompile]
    [WithAll(typeof(PeriodEffect))]
    [WithAny(typeof(DurationEffect), typeof(InfiniteEffect))]
    [WithNone(typeof(EndTimeComponent))]
    public partial struct CreatePeriodInstanceEffectJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;

        void Execute([EntityIndexInQuery] int entityInQueryIndex, in AffectedTargetComponent affectedTarget, in DynamicBuffer<ModifierAggregatorData> statModifierEntityElementBuffers, in ActivatedStateEntityOwner activatedStateEntityOwner)
        {
            // create a period instance effect
            var periodInstanceEntity = this.Ecb.CreateEntity(entityInQueryIndex);
            this.Ecb.AddComponent<PeriodEffectInstanceTag>(entityInQueryIndex, periodInstanceEntity);
            this.Ecb.AddComponent(entityInQueryIndex, periodInstanceEntity, affectedTarget);
            var cloneBuffer = this.Ecb.AddBuffer<ModifierAggregatorData>(entityInQueryIndex, periodInstanceEntity);
            foreach (var statModifierEntityElement in statModifierEntityElementBuffers)
            {
                cloneBuffer.Add(statModifierEntityElement);
            }
            this.Ecb.AddComponent(entityInQueryIndex, periodInstanceEntity, activatedStateEntityOwner);
        }
    }
}