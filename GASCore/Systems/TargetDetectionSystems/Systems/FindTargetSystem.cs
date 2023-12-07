namespace GASCore.Systems.TargetDetectionSystems.Systems
{
    using GASCore.Systems.LogicEffectSystems.Components;
    using GASCore.Systems.StatSystems.Components;
    using GASCore.Systems.TargetDetectionSystems.Components;
    using GASCore.Systems.TimelineSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;
    using Unity.Transforms;

    [UpdateInGroup(typeof(FindTargetGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct FindTargetSystem : ISystem
    {
        private EntityQuery entityQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state) { this.entityQuery = SystemAPI.QueryBuilder().WithAll<LocalToWorld, StatNameToIndex, TeamOwnerId>().WithNone<UntargetableTag>().Build(); }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var entities = this.entityQuery.ToEntityArray(state.WorldUpdateAllocator);

            new FindTargetJob
            {
                Entities = entities
            }.ScheduleParallel();
        }
    }

    [WithAll(typeof(FindTargetComponent))]
    [WithNone(typeof(OverrideFindTargetTag))]
    [BurstCompile]
    public partial struct FindTargetJob : IJobEntity
    {
        [ReadOnly] public NativeArray<Entity> Entities;

        private void Execute(ref DynamicBuffer<TargetableElement> targetables)
        {
            targetables.Clear();
            foreach (var entity in this.Entities)
            {
                targetables.Add(entity);
            }
        }
    }
}