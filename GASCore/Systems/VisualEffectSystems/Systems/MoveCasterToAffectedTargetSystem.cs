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
    public partial struct MoveCasterToAffectedTargetSystem : ISystem
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
            var lifeTimeJob = new MoveCasterToAffectedTargetJob()
            {
                Ecb            = ecb,
                PositionLookup = this.positionLookup
            };
            lifeTimeJob.ScheduleParallel();
        }
    }

    [BurstCompile]
    [WithAll(typeof(MoveCasterToAffectedTarget))]
    [WithChangeFilter(typeof(AffectedTargetComponent))]
    public partial struct MoveCasterToAffectedTargetJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;

        [ReadOnly] public ComponentLookup<LocalToWorld> PositionLookup;
        void Execute([EntityInQueryIndex] int entityInQueryIndex, in CasterComponent caster, in AffectedTargetComponent affectedTarget, in MoveCasterToAffectedTarget moveCasterToAffectedTarget)
        {
            if (moveCasterToAffectedTarget.IsChase)
            {
                this.Ecb.AddComponent(entityInQueryIndex, caster.Value, new ChaseTargetEntity() { Value = affectedTarget.Value });
            }

            this.Ecb.AddComponent(entityInQueryIndex, caster.Value, new TargetPosition() { Value = this.PositionLookup[affectedTarget.Value].Position });
        }
    }
}