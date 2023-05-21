namespace GASCore.Systems.VisualEffectSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using GASCore.Systems.VisualEffectSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [UpdateInGroup(typeof(AbilityVisualEffectGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct MoveToAffectedTargetSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { state.RequireForUpdate<MoveToAffectedTarget>(); }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<BeginPresentationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            new MoveToAffectedTargetJob()
            {
                Ecb            = ecb,
                PositionLookup = SystemAPI.GetComponentLookup<LocalToWorld>(true)
            }.ScheduleParallel();
        }
    }

    [BurstCompile]
    [WithAll(typeof(MoveToAffectedTarget))]
    [WithChangeFilter(typeof(AffectedTargetComponent))]
    public partial struct MoveToAffectedTargetJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;

        [ReadOnly] public ComponentLookup<LocalToWorld> PositionLookup;
        void Execute([EntityIndexInQuery] int entityInQueryIndex, in AffectedTargetComponent affectedTarget,in MoveToAffectedTarget data, in SourceComponent source)
        {
            this.Ecb.AddComponent(entityInQueryIndex, source, new MovementDirection(){Value = math.normalize(this.PositionLookup[affectedTarget.Value].Position - this.PositionLookup[source.Value].Position)});
            
            if(data.RotateSpeed <= 0) return;
            this.Ecb.AddComponent(entityInQueryIndex, source, new RotationSpeed()
            {
                Value = data.RotateSpeed
            });
        }
    }
}