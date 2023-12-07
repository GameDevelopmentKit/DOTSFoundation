namespace GASCore.Systems.LogicEffectSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;

    [UpdateInGroup(typeof(GameAbilityBeginSimulationSystemGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct AddUntargetableTagSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { state.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<MarkUntargetable>().WithNone<WaitMarkUntargetableCleanup>().Build()); }


        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            new AddUntargetableTagJob()
            {
                Ecb                = ecb,
                UntargetableLookup = SystemAPI.GetComponentLookup<UntargetableTag>(true)
            }.ScheduleParallel();
        }
    }


    [WithAll(typeof(MarkUntargetable))]
    [WithNone(typeof(WaitMarkUntargetableCleanup))]
    public partial struct AddUntargetableTagJob : IJobEntity
    {
        public            EntityCommandBuffer.ParallelWriter Ecb;
        [ReadOnly] public ComponentLookup<UntargetableTag>   UntargetableLookup;
        void Execute(Entity entity, [EntityIndexInQuery] int entityInQueryIndex, in AffectedTargetComponent affectedTarget)
        {
            this.Ecb.AddComponent(entityInQueryIndex, entity, new WaitMarkUntargetableCleanup() { AffectedTargetEntity = affectedTarget });
            if (this.UntargetableLookup.HasComponent(affectedTarget))
                this.Ecb.SetComponentEnabled<UntargetableTag>(entityInQueryIndex, affectedTarget, true);
            else
                this.Ecb.AddComponent<UntargetableTag>(entityInQueryIndex, affectedTarget);
        }
    }
}