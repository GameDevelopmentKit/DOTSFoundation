namespace GASCore.Systems.LogicEffectSystems.Systems
{
    using DOTSCore.Extension;
    using GASCore.Blueprints;
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.CommonSystems.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using GASCore.Systems.TimelineSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;
    using Unity.Mathematics;
    using Unity.Transforms;

    [UpdateInGroup(typeof(AbilityTimelineGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial class FindNearestTargetSystem : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem endSimEcbSystem;
        private EntityQuery                            teamQuery;
        private ComponentLookup<TeamOwnerId>           teamOwnerLookup;
        private ComponentLookup<Translation>           translationLookup;
        private ComponentLookup<TagComponent>          tagLookup;

        protected override void OnCreate()
        {
            this.endSimEcbSystem = this.World.GetExistingSystemManaged<EndSimulationEntityCommandBufferSystem>();
            using var queryBuilder = new EntityQueryBuilder(Allocator.Temp).WithAll<TeamOwnerId, Translation>();
            this.teamOwnerLookup   = this.GetComponentLookup<TeamOwnerId>(true);
            this.translationLookup = this.GetComponentLookup<Translation>(true);
            this.tagLookup         = this.GetComponentLookup<TagComponent>(true);
            this.teamQuery         = this.GetEntityQuery(queryBuilder);
        }

        protected override void OnUpdate()
        {
            var ecb                    = this.endSimEcbSystem.CreateCommandBuffer().AsParallelWriter();
            var entityList             = this.teamQuery.ToEntityListAsync(this.WorldUpdateAllocator, out var getTargetEntityJobHandle);
            var localTeamOwnerLookup   = this.teamOwnerLookup.UpdateComponentLookup(this);
            var localTranslationLookup = this.translationLookup.UpdateComponentLookup(this);
            var localTagLookup         = this.tagLookup.UpdateComponentLookup(this);
            this.Dependency = this.Entities.WithBurst().WithChangeFilter<FindNearestTarget>().WithReadOnly(entityList).WithReadOnly(localTeamOwnerLookup).WithReadOnly(localTranslationLookup).WithReadOnly(localTagLookup)
                .ForEach((Entity triggerEntity, int entityInQueryIndex, ref DynamicBuffer<TargetableElement> targetBuffer, in TriggerConditionCount triggerConditionCount, in CasterComponent caster,
                    in DynamicBuffer<TargetTypeElement> targetTypeBuffer, in FindNearestTarget findNearestTarget) =>
                {
                    var casterPosition = localTranslationLookup[caster.Value].Value;
                    var casterTeamId   = localTeamOwnerLookup[caster.Value].Value;

                    var    minDist      = float.MaxValue;
                    Entity targetEntity = default;
                    foreach (var tempTargetEntity in entityList)
                    {
                        if (findNearestTarget.IsIncludedTag)
                        {
                            if(!localTagLookup.HasComponent(tempTargetEntity)) continue;
                            if (!findNearestTarget.TargetTag.Equals(localTagLookup[tempTargetEntity].Value)) continue;
                        }
                        
                        foreach (var targetTypeElement in targetTypeBuffer)
                        {
                            var isTargetableEntity = false;
                            switch (targetTypeElement.Value)
                            {
                                case TargetType.Opponent:
                                    if (localTeamOwnerLookup[tempTargetEntity].Value != casterTeamId) isTargetableEntity = true;
                                    break;
                                case TargetType.Caster:
                                    if (tempTargetEntity == caster.Value) isTargetableEntity = true;
                                    break;
                                case TargetType.Ally:
                                    if (localTeamOwnerLookup[tempTargetEntity].Value == casterTeamId && tempTargetEntity != caster.Value) isTargetableEntity = true;
                                    break;
                            }

                            if (!isTargetableEntity) continue;
                            var distancesq = math.distancesq(casterPosition, localTranslationLookup[tempTargetEntity].Value);

                            if (minDist > distancesq)
                            {
                                minDist      = distancesq;
                                targetEntity = tempTargetEntity;
                            }

                            break;
                        }
                    }

                    if (!targetEntity.Equals(default))
                    {
                        targetBuffer.Add(new TargetableElement() { Value                                            = targetEntity });
                        ecb.SetComponent(entityInQueryIndex, triggerEntity, new TriggerConditionCount() { Value = triggerConditionCount.Value - 1 });
                    }
                }).ScheduleParallel(JobHandle.CombineDependencies(getTargetEntityJobHandle, this.Dependency));
        }
    }
}