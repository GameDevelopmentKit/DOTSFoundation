namespace GASCore.Systems.EntityGeneratorSystems.Systems
{
    using DOTSCore.Extension;
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.EntityGeneratorSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Transforms;

    [UpdateInGroup(typeof(GameAbilityBeginSimulationSystemGroup))]
    [UpdateAfter(typeof(SetupInitialPositionSystem))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct UnAttachFromParentSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { state.RequireForUpdate<UnAttachFromParent>(); }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            new UnAttachFromParentJob()
            {
                Ecb                = ecb,
                ParentLookup       = SystemAPI.GetComponentLookup<Parent>(true),
                LocalToWorldLookup = SystemAPI.GetComponentLookup<LocalToWorld>(true)
            }.ScheduleParallel();
        }
    }


    [WithAll(typeof(UnAttachFromParent))]
    [BurstCompile]
    public partial struct UnAttachFromParentJob : IJobEntity
    {
        public            EntityCommandBuffer.ParallelWriter Ecb;
        [ReadOnly] public ComponentLookup<Parent>            ParentLookup;
        [ReadOnly] public ComponentLookup<LocalToWorld>      LocalToWorldLookup;
        private void Execute([EntityIndexInQuery] int index, in AffectedTargetComponent affectedTarget)
        {
            if (this.ParentLookup.HasComponent(affectedTarget.Value))
            {
                this.Ecb.RemoveParent(index, affectedTarget.Value);
                var localToWorld = this.LocalToWorldLookup[affectedTarget.Value];
                this.Ecb.SetComponent(index, affectedTarget.Value, LocalTransform.FromMatrix(localToWorld.Value));
            }
        }
    }
}