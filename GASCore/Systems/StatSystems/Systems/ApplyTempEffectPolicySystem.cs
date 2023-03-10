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
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct ApplyTempEffectPolicySystem : ISystem
    {
        private BufferLookup<LinkedEntityGroup> linkedEntityLookup;
        [BurstCompile]
        public void OnCreate(ref SystemState state) { this.linkedEntityLookup = state.GetBufferLookup<LinkedEntityGroup>(true); }
        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            new ApplyDurationEffectPolicyJob() { Ecb = ecb }.ScheduleParallel();
            new ApplyInfiniteEffectPolicyJob() { Ecb = ecb }.ScheduleParallel();

            this.linkedEntityLookup.Update(ref state);
            new LinkTempEffectToAffectedTargetJob()
            {
                Ecb                = ecb,
                LinkedEntityLookup = this.linkedEntityLookup
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

    [BurstCompile]
    [WithAny(typeof(InfiniteEffect), typeof(DurationEffect))]
    [WithNone(typeof(IgnoreCleanupTag), typeof(Duration))]
    public partial struct LinkTempEffectToAffectedTargetJob : IJobEntity
    {
        public            EntityCommandBuffer.ParallelWriter Ecb;
        [ReadOnly] public BufferLookup<LinkedEntityGroup>    LinkedEntityLookup;
        void Execute(Entity statModifierEntity, [EntityIndexInQuery] int entityInQueryIndex, in AffectedTargetComponent affectedTarget)
        {
            if (!LinkedEntityLookup.HasBuffer(affectedTarget)) this.Ecb.AddBuffer<LinkedEntityGroup>(entityInQueryIndex, affectedTarget);
            this.Ecb.AppendToBuffer(entityInQueryIndex, affectedTarget, new LinkedEntityGroup() { Value = statModifierEntity });
        }
    }
}