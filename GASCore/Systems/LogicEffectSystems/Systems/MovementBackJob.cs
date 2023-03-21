namespace GASCore.Systems.LogicEffectSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    public partial struct MovementBackJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<LocalTransform>     targetLocalTransform;
        [ReadOnly] public ComponentLookup<IgnoreKnockBackTag> IgnoreKnockBackLookup;
        public            EntityCommandBuffer.ParallelWriter  Ecb;

        void Execute([EntityIndexInQuery] int entityInQueryIndex, in MoveBackComponent moveBackComponent, in AffectedTargetComponent affectedTargetComponent, in CasterComponent casterComponent)
        {
            if (this.IgnoreKnockBackLookup.HasComponent(affectedTargetComponent.Value)) return;

            LocalTransform targetTransform = this.targetLocalTransform[affectedTargetComponent.Value];
            LocalTransform casterTransform = this.targetLocalTransform[casterComponent.Value];
            float3         direction       = casterTransform._Position - targetTransform.Position;
            targetTransform.Position -= math.normalize(direction) * moveBackComponent.PushBackForce;
            this.Ecb.SetComponent(entityInQueryIndex, affectedTargetComponent.Value, targetTransform);
        }
    }

    [UpdateInGroup(typeof(AbilityLogicEffectGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct MovementBackSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MoveBackComponent>();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            new MovementBackJob()
            {
                Ecb                   = ecb,
                targetLocalTransform  = SystemAPI.GetComponentLookup<LocalTransform>(true),
                IgnoreKnockBackLookup = SystemAPI.GetComponentLookup<IgnoreKnockBackTag>(true),
            }.ScheduleParallel();
        }
    }
}