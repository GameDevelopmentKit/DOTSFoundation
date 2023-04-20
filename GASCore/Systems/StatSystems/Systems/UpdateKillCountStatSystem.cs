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
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct UpdateKillCountStatSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<UpdateKillCountStat>();
        }

        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new UpdateKillCountJob
            {
                StatNameLookup = SystemAPI.GetComponentLookup<UpdateKillCountStatNameComponent>(true),
            }.ScheduleParallel(state.Dependency);
        }
    }

    [WithChangeFilter(typeof(UpdateKillCountStat))]
    [BurstCompile]
    public partial struct UpdateKillCountJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<UpdateKillCountStatNameComponent> StatNameLookup;

        private void Execute(ref DynamicBuffer<ModifierAggregatorData> modifierAggregatorBuffer, in CasterComponent caster)
        {
            modifierAggregatorBuffer.Add(new ModifierAggregatorData()
            {
                TargetStat = this.StatNameLookup[caster],
                Add        = -1,
                Multiply   = 1,
                Divide     = 1,
            });
        }
    }
}