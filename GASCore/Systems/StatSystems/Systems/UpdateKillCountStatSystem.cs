namespace GASCore.Systems.StatSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.StatSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityLogicEffectGroup))]
    [UpdateAfter(typeof(AggregateStatModifierSystem))]
    [UpdateBefore(typeof(ApplyInstantStatModifierSystem))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct UpdateKillCountStatSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { state.RequireForUpdate<UpdateKillCountStat>(); }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new UpdateKillCountJob
            {
                StatNameLookup = SystemAPI.GetComponentLookup<UpdateKillCountStatNameComponent>(true),
            }.ScheduleParallel(state.Dependency);
        }
    }

    [WithAll(typeof(UpdateKillCountStat))]
    [WithChangeFilter(typeof(UpdateKillCountStat))]
    [BurstCompile]
    public partial struct UpdateKillCountJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<UpdateKillCountStatNameComponent> StatNameLookup;

        private void Execute(ref DynamicBuffer<ModifierAggregatorData> modifierAggregatorBuffer, in CasterComponent caster)
        {
            modifierAggregatorBuffer.Add(new ModifierAggregatorData(this.StatNameLookup[caster], -1));
        }
    }
}