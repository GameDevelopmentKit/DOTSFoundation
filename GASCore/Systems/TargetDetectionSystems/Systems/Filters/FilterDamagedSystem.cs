namespace GASCore.Systems.TargetDetectionSystems.Systems.Filters
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.StatSystems.Components;
    using GASCore.Systems.TargetDetectionSystems.Components;
    using GASCore.Systems.TargetDetectionSystems.Components.Filters;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;


    [UpdateInGroup(typeof(AbilityVisualEffectGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct FilterDamagedSystem : ISystem
    {
        private EntityQuery damagedEffectEntityQuery;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            this.damagedEffectEntityQuery = SystemAPI.QueryBuilder().WithAll<AffectedTargetComponent, DealDamageTag>().Build();
            state.RequireForUpdate(this.damagedEffectEntityQuery);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new FilterDamagedJob
            {
                AffectedTargetEntities = this.damagedEffectEntityQuery.ToComponentDataArray<AffectedTargetComponent>(Allocator.TempJob)
            }.ScheduleParallel();
        }
    }

    [WithAll(typeof(FindTargetComponent))]
    [WithAll(typeof(FilterDamagedTag))]
    [BurstCompile]
    public partial struct FilterDamagedJob : IJobEntity
    {
        [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<AffectedTargetComponent> AffectedTargetEntities;

        private void Execute(ref DynamicBuffer<TargetableElement> targetables)
        {
            targetables.Clear();
            foreach (var entity in this.AffectedTargetEntities)
            {
                targetables.Add(new TargetableElement() { Value = entity });
            }
        }
    }
}