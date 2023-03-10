namespace GASCore.Systems.StatSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using GASCore.Systems.StatSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityLogicEffectGroup))]
    [UpdateAfter(typeof(AggregateStatModifierSystem))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct UpdateKillCountSystem : ISystem
    {
        private ComponentLookup<TagComponent> tagLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            this.tagLookup = state.GetComponentLookup<TagComponent>(true);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            this.tagLookup.Update(ref state);

            new UpdateKillCountJob()
            {
                TagLookup = this.tagLookup,
            }.ScheduleParallel();
        }
    }

    [WithChangeFilter(typeof(UpdateKillCountTag))]
    [BurstCompile]
    public partial struct UpdateKillCountJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<TagComponent> TagLookup;

        private void Execute(ref DynamicBuffer<ModifierAggregatorData> modifierAggregatorBuffer, in CasterComponent caster)
        {
            modifierAggregatorBuffer.Add(new ModifierAggregatorData()
            {
                TargetStat = this.TagLookup[caster],
                Add        = -1,
                Multiply   = 1,
                Divide     = 1,
            });
        }
    }
}