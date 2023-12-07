namespace GASCore.Systems.LogicEffectSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.LogicEffectSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;

    [UpdateInGroup(typeof(GameAbilityBeginSimulationSystemGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct RemoveUntargetableTagSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { state.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<WaitMarkUntargetableCleanup>().WithNone<MarkUntargetable>().Build()); }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            new RemoveUntargetableTagJob()
            {
                Ecb                = ecb,
                UntargetableLookup = SystemAPI.GetComponentLookup<UntargetableTag>(true)
            }.ScheduleParallel();
        }
    }

    [WithAll(typeof(WaitMarkUntargetableCleanup))]
    [WithNone(typeof(MarkUntargetable))]
    public partial struct RemoveUntargetableTagJob : IJobEntity
    {
        public            EntityCommandBuffer.ParallelWriter Ecb;
        [ReadOnly] public ComponentLookup<UntargetableTag>   UntargetableLookup;
        void Execute(Entity entity, [EntityIndexInQuery] int entityInQueryIndex, in WaitMarkUntargetableCleanup markUntargetableCleanup)
        {
            if (this.UntargetableLookup.HasComponent(markUntargetableCleanup.AffectedTargetEntity))
                this.Ecb.SetComponentEnabled<UntargetableTag>(entityInQueryIndex, markUntargetableCleanup.AffectedTargetEntity, false);
            this.Ecb.RemoveComponent<WaitMarkUntargetableCleanup>(entityInQueryIndex, entity);
        }
    }
}