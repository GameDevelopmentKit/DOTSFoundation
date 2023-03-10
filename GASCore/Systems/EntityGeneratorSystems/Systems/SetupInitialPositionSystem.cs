namespace GASCore.Systems.EntityGeneratorSystems.Systems
{
    using DOTSCore.CommonSystems.Components;
    using DOTSCore.Extension;
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.EntityGeneratorSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;
    using Random = Unity.Mathematics.Random;

    // [UpdateInGroup(typeof(AbilityVisualEffectGroup))]
    // [RequireMatchingQueriesForUpdate]
    // public partial class SetupInitialPositionSystem : SystemBase
    // {
    //     private AbilityPresentEntityCommandBufferSystem beginSimEcbSystem;
    //
    //     protected override void OnCreate() { this.beginSimEcbSystem = this.World.GetExistingSystemManaged<AbilityPresentEntityCommandBufferSystem>(); }
    //     protected override void OnUpdate()
    //     {
    //         var ecb = this.beginSimEcbSystem.CreateCommandBuffer().AsParallelWriter();
    //         this.Entities.WithAny<AttachToAffectedTarget, PositionOffset, RandomPositionOffset>().WithNone<GameObjectHybridLink>().WithChangeFilter<AffectedTargetComponent>().ForEach(
    //             (Entity actionEntity, int entityInQueryIndex, ref LocalTransform transform, in AffectedTargetComponent affectedTarget) =>
    //             {
    //                 if (SystemAPI.HasComponent<AttachToAffectedTarget>(actionEntity))
    //                 {
    //                     transform.Position = float3.zero;
    //                     ecb.SetParent(entityInQueryIndex, actionEntity, affectedTarget.Value);
    //                 }
    //
    //                 if (SystemAPI.HasComponent<PositionOffset>(actionEntity))
    //                 {
    //                     var offsetPos = SystemAPI.GetComponent<PositionOffset>(actionEntity);
    //                     var forward   = transform.Forward();
    //                     transform.Position += new float3(offsetPos.Value.x * forward.x, offsetPos.Value.y, offsetPos.Value.z * forward.z);
    //                 }
    //
    //                 if (SystemAPI.HasComponent<RandomPositionOffset>(actionEntity))
    //                 {
    //                     var rnd                  = Random.CreateFromIndex((uint)(entityInQueryIndex + 1));
    //                     var randomPositionOffset = SystemAPI.GetComponent<RandomPositionOffset>(actionEntity);
    //                     transform.Position += rnd.NextFloat3(randomPositionOffset.Min, randomPositionOffset.Max);
    //                 }
    //             }).ScheduleParallel();
    //         this.beginSimEcbSystem.AddJobHandleForProducer(this.Dependency);
    //     }
    // }
    //

    [UpdateInGroup(typeof(GameAbilityBeginSimulationSystemGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct SetupInitialPositionSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new RandomPositionOffsetJob().ScheduleParallel(state.Dependency);
            state.Dependency = new PositionOffsetJob().ScheduleParallel(state.Dependency);
        }
    }

    [WithChangeFilter(typeof(RandomPositionOffset))]
    [BurstCompile]
    public partial struct RandomPositionOffsetJob : IJobEntity
    {
        private void Execute([EntityIndexInQuery] int index, in RandomPositionOffset randomPositionOffset, ref PositionOffset positionOffset)
        {
            var rnd = Random.CreateFromIndex((uint)(index + 1));
            positionOffset.Value = rnd.NextFloat3(randomPositionOffset.Min, randomPositionOffset.Max);
        }
    }

    [WithNone(typeof(AttachPositionToAffectedTarget))]
    [WithChangeFilter(typeof(PositionOffset))]
    [BurstCompile]
    public partial struct PositionOffsetJob : IJobEntity
    {
        private void Execute(in PositionOffset positionOffset, ref LocalTransform localTransform) { localTransform.Position += positionOffset.Value; }
    }
}

// Instantiate ability effect


// setup position
// attach pos, set parent
// sync point

// sync transform


// Generate entity