namespace GASCore.Systems.TargetDetectionSystems.Systems
{
    using GASCore.Blueprints;
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using GASCore.Systems.TargetDetectionSystems.Components;
    using GASCore.Systems.TimelineSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityTimelineGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct FilterTargetableWithTypeSystem : ISystem
    {
        private ComponentLookup<TeamOwnerId> teamLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state) { this.teamLookup = state.GetComponentLookup<TeamOwnerId>(true); }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            this.teamLookup.Update(ref state);

            new FilterTargetablewithTypeJob() { TeamLookup = this.teamLookup }.ScheduleParallel();
        }
    }

    [BurstCompile]
    [WithChangeFilter(typeof(TargetableElement))]
    public partial struct FilterTargetablewithTypeJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<TeamOwnerId> TeamLookup;

        private void Execute(
            in CasterComponent caster,
            in DynamicBuffer<TargetTypeElement> targetableTypes,
            ref DynamicBuffer<TargetableElement> targets,
            ref DynamicBuffer<CompletedTriggerElement> completedTriggerBuffer)
        {
            var casterTeam = this.TeamLookup[caster.Value].Value;
            for (var index = 0; index < targets.Length;)
            {
                var target = targets[index].Value;

                if (!this.TeamLookup.HasComponent(target))
                {
                    targets.RemoveAtSwapBack(index);
                    continue;
                }
                else
                {
                    index++;
                }

                var targetTeam = this.TeamLookup[target].Value;
                // Debug.Log($"FilterTargetableJob target {target.Index} in Team {targetTeam}, caster team {casterTeam}");

                var hasTargetable = false;

                foreach (var targetType in targetableTypes)
                {
                    switch (targetType.Value)
                    {
                        case TargetType.Opponent:
                            if (casterTeam != targetTeam) hasTargetable = true;
                            break;
                        case TargetType.Caster:
                            if (caster.Value == target) hasTargetable = true;
                            break;
                        case TargetType.Ally:
                            if (casterTeam == targetTeam && caster.Value != target) hasTargetable = true;
                            break;
                    }

                    if (!hasTargetable) continue;
                    completedTriggerBuffer.Add(new CompletedTriggerElement() { Index = TypeManager.GetTypeIndex<TargetTypeElement>() });
                    break;
                }
            }
        }
    }
}