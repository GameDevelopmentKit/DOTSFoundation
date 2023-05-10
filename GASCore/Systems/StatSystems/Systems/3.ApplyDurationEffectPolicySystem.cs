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
    [UpdateAfter(typeof(AggregateStatModifierSystem))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct ApplyDurationEffectPolicySystem : ISystem
    {

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton       = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb                = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            
            new ApplyDurationEffectPolicyJob() { Ecb = ecb }.ScheduleParallel();
            new ApplyInfiniteEffectPolicyJob() { Ecb = ecb }.ScheduleParallel();
            new ApplyPeriodEffectPolicyJob()
            {
                Ecb                = ecb,
                CurrentElapsedTime = SystemAPI.Time.ElapsedTime
            }.ScheduleParallel();
        }
    }

    [BurstCompile]
    [WithNone(typeof(Duration))]
    public partial struct ApplyDurationEffectPolicyJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;

        void Execute(Entity statModifierEntity, [EntityIndexInQuery] int entityInQueryIndex, in DurationEffect durationEffect)
        {
            this.Ecb.AddComponent(entityInQueryIndex, statModifierEntity, new Duration() { Value = durationEffect.Value });
        }
    }

    [BurstCompile]
    [WithAll(typeof(InfiniteEffect))]
    [WithNone(typeof(IgnoreCleanupTag))]
    public partial struct ApplyInfiniteEffectPolicyJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;

        void Execute(Entity statModifierEntity, [EntityIndexInQuery] int entityInQueryIndex) { this.Ecb.AddComponent<IgnoreCleanupTag>(entityInQueryIndex, statModifierEntity); }
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
}