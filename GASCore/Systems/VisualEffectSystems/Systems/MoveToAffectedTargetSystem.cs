namespace GASCore.Systems.VisualEffectSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using GASCore.Systems.VisualEffectSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Transforms;

    [UpdateInGroup(typeof(AbilityVisualEffectGroup))]
    [UpdateAfter(typeof(SetupInitialPositionSystem))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct MoveToAffectedTargetSystem : ISystem
    {
        ComponentLookup<LocalToWorld> positionLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state) { this.positionLookup = state.GetComponentLookup<LocalToWorld>(true); }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<AbilityPresentEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            this.positionLookup.Update(ref state);
            var lifeTimeJob = new MoveToAffectedTargetJob()
            {
                Ecb            = ecb,
                PositionLookup = this.positionLookup
            };
            lifeTimeJob.ScheduleParallel();
        }
    }

    [BurstCompile]
    [WithAll(typeof(MoveToAffectedTarget))]
    [WithChangeFilter(typeof(AffectedTargetComponent))]
    public partial struct MoveToAffectedTargetJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;

        [ReadOnly] public ComponentLookup<LocalToWorld> PositionLookup;
        void Execute([EntityInQueryIndex] int entityInQueryIndex, in AffectedTargetComponent affectedTarget, in SourceComponent source)
        {
            this.Ecb.AddComponent(entityInQueryIndex, source.Value, new TargetPosition(this.PositionLookup[affectedTarget.Value].Position));
        }
    }
}