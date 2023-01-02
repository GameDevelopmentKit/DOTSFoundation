namespace GASCore.Systems.VisualEffectSystems.Systems
{
    using DOTSCore.CommonSystems.Components;
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.VisualEffectSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [UpdateInGroup(typeof(AbilityVisualEffectGroup))]
    [UpdateAfter(typeof(SetupInitialPositionSystem))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct MoveCasterToAffectedTargetSystem : ISystem
    {
        ComponentLookup<LocalToWorld> positionLookup;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state) {  this.positionLookup = state.GetComponentLookup<LocalToWorld>(true);  }

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
                Ecb                     = ecb,
                PositionLookup = this.positionLookup
            };
            lifeTimeJob.ScheduleParallel();
        }
    }

    [BurstCompile]
    [WithAll(typeof(MoveCasterToAffectedTarget))]
    [WithNone(typeof(MovementDirection))]
    public partial struct MoveCasterToAffectedTargetJob : IJobEntity
    {
        public            EntityCommandBuffer.ParallelWriter Ecb;
        
        [ReadOnly] public ComponentLookup<LocalToWorld> PositionLookup;
        void Execute([EntityInQueryIndex] int entityInQueryIndex, in CasterComponent caster,in AffectedTargetComponent affectedTarget)
        {
            var dir = this.PositionLookup[affectedTarget.Value].Position - this.PositionLookup[caster.Value].Position;
            this.Ecb.AddComponent(entityInQueryIndex, caster.Value, new MovementDirection() { Value = math.normalize(dir)});
        }
    }
}