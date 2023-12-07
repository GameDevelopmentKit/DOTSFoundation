namespace GASCore.Systems.EntityGeneratorSystems.Systems
{
    using DOTSCore.Extension;
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.EntityGeneratorSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [UpdateInGroup(typeof(GameAbilityBeginSimulationSystemGroup))]
    [UpdateAfter(typeof(SetupInitialPositionSystem))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct AttachToAffectedTargetSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { state.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<AttachToAffectedTarget>().WithNone<Parent>().Build()); }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            new AttachToAffectedTargetJob()
            {
                Ecb = ecb ,
                PositionOffsetLookup = SystemAPI.GetComponentLookup<PositionOffset>(true)
            }.ScheduleParallel();
        }
    }


    [WithAll(typeof(AttachToAffectedTarget))]
    [WithNone(typeof(Parent))]
    [BurstCompile]
    public partial struct AttachToAffectedTargetJob : IJobEntity
    {
        
        public            EntityCommandBuffer.ParallelWriter Ecb;
        [ReadOnly] public ComponentLookup<PositionOffset>    PositionOffsetLookup;
        private void Execute(in Entity entity, [EntityIndexInQuery] int index, in AffectedTargetComponent affectedTarget, ref LocalTransform localTransform)
        {
            localTransform.Position = this.PositionOffsetLookup.HasComponent(entity) ? this.PositionOffsetLookup[entity].Value : float3.zero;
            Ecb.SetParent(index, entity, affectedTarget.Value);
        }
    }
}