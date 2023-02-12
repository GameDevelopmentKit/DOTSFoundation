namespace GASCore.Systems.LogicEffectSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;

    [BurstCompile]
    [WithNone(typeof(SourceComponent))]
    public partial struct SetSourceEntityJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;
        void Execute(Entity entity, [EntityInQueryIndex] int index, SourceTypeComponent sourceType, in AffectedTargetComponent affectedTarget, CasterComponent caster)
        {
            switch (sourceType.Value)
            {
                case SourceType.Self:
                    this.Ecb.AddComponent(index, entity, new SourceComponent() { Value = entity });
                    break;
                case SourceType.Caster:
                    this.Ecb.AddComponent(index, entity, new SourceComponent() { Value = caster.Value });
                    break;
                case SourceType.AffectedTarget:
                    this.Ecb.AddComponent(index, entity, new SourceComponent() { Value = affectedTarget.Value });
                    break;
            }
        }
    }

    [BurstCompile]
    [WithAll(typeof(AbilityEffectId))]
    [WithNone(typeof(SourceComponent), typeof(SourceTypeComponent))]
    public partial struct SetDefaultSourceEntityJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;

        void Execute(Entity entity, [EntityInQueryIndex] int index) { this.Ecb.AddComponent(index, entity, new SourceComponent() { Value = entity }); }
    }

    [UpdateInGroup(typeof(AbilityCommonSystemGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct SetSourceEntitySystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb         = new EntityCommandBuffer(Allocator.TempJob);
            var ecbParallel = ecb.AsParallelWriter();

            state.Dependency = new SetSourceEntityJob() { Ecb        = ecbParallel }.ScheduleParallel(state.Dependency);
            state.Dependency = new SetDefaultSourceEntityJob() { Ecb = ecbParallel }.ScheduleParallel(state.Dependency);
            state.Dependency.Complete();
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}