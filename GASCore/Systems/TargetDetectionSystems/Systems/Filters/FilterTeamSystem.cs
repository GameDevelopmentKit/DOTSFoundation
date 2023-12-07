namespace GASCore.Systems.TargetDetectionSystems.Systems.Filters
{
    using System;
    using GASCore.Blueprints;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using GASCore.Systems.StatSystems.Components;
    using GASCore.Systems.TargetDetectionSystems.Components;
    using GASCore.Systems.TargetDetectionSystems.Components.Filters;
    using GASCore.Systems.TimelineSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;

    [UpdateInGroup(typeof(FilterTargetGroup), OrderFirst = true)]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct FilterTeamSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { state.RequireForUpdate<FindTargetComponent>(); }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) { new FilterTeamJob { TeamLookup = SystemAPI.GetComponentLookup<TeamOwnerId>(true), }.ScheduleParallel(); }
    }

    [WithAll(typeof(FindTargetComponent))]
    [WithAll(typeof(FilterTeam))]
    [BurstCompile]
    public partial struct FilterTeamJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<TeamOwnerId> TeamLookup;

        private void Execute(ref DynamicBuffer<TargetableElement> targetables, in FilterTeam filterTeam, in CasterComponent caster)
        {
            for (var i = 0; i < targetables.Length;)
            {
                if (this.TeamLookup.TryGetComponent(targetables[i], out var targetTeamId))
                {
                    var casterTeamId = this.TeamLookup[caster].Value;
                    switch (filterTeam.Value)
                    {
                        case TargetType.Opponent when targetTeamId.Value != casterTeamId:
                        case TargetType.Caster when caster.Value.Equals(targetables[i]):
                        case TargetType.Ally when targetTeamId.Value == casterTeamId:
                        {
                            ++i;
                            continue;
                        }
                    }
                }

                targetables.RemoveAtSwapBack(i);
            }
        }
    }
}