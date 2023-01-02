namespace GASCore.Systems.LogicEffectSystems.Systems
{
    using DOTSCore.Extension;
    using GASCore.Blueprints;
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.CommonSystems.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;
    using Unity.Mathematics;
    using Unity.Transforms;
    using UnityEngine;

    [UpdateInGroup(typeof(AbilityLogicEffectGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial class TrackingTargetEntityInCastRangeSystem : SystemBase
    {
        private EntityQuery                            teamQuery;
        private EndSimulationEntityCommandBufferSystem endSimEcbSystem;

        protected override void OnCreate()
        {
            this.endSimEcbSystem = this.World.GetExistingSystemManaged<EndSimulationEntityCommandBufferSystem>();
            using var queryBuilder = new EntityQueryBuilder(Allocator.Temp).WithAll<TeamOwnerId, Translation>();
            this.teamQuery = this.GetEntityQuery(queryBuilder);
        }

        protected override void OnUpdate()
        {
            var ecb        = this.endSimEcbSystem.CreateCommandBuffer().AsParallelWriter();
            var entityList = this.teamQuery.ToEntityListAsync(this.WorldUpdateAllocator, out var getTargetEntityJobHandle);
            var teamIds    = this.teamQuery.ToComponentDataListAsync<TeamOwnerId>(this.WorldUpdateAllocator, out var getTeamIdJobHandle);
            var positions  = this.teamQuery.ToComponentDataListAsync<Translation>(this.WorldUpdateAllocator, out var getPositionJobHandle);
            var jobCombine = JobHandle.CombineDependencies(new NativeArray<JobHandle>(4, Allocator.Temp)
                { [0] = getTargetEntityJobHandle, [1] = getTeamIdJobHandle, [2] = getPositionJobHandle, [3] = this.Dependency });

            this.Dependency = this.Entities.WithBurst().WithAll<NeedToTrackingTargetInCastRange>().WithReadOnly(entityList).WithReadOnly(teamIds).WithReadOnly(positions)
                .ForEach((Entity activatedStateAbilityEntity, int entityInQueryIndex, ref DynamicBuffer<EntityInAbilityRangeElement> entityInRangeBuffer,
                    in CastRangeComponent castRange, in CasterComponent caster, in DynamicBuffer<TargetTypeElement> targetTypeBuffer) =>
                {
                    var casterPosition = GetComponent<Translation>(caster.Value).Value;
                    var casterTeamId   = GetComponent<TeamOwnerId>(caster.Value).Value;
                    var sqrRange       = castRange.Value * castRange.Value;

                    for (int i = 0; i < positions.Length; i++)
                    {
                        // search index of entity in entityInRangeBuffer
                        int indexInEntityRangeBuffer = -1;
                        var targetEntity             = entityList[i];
                        for (var index = 0; index < entityInRangeBuffer.Length; index++)
                        {
                            var element = entityInRangeBuffer[index];
                            if (element.Value.Equals(targetEntity))
                            {
                                indexInEntityRangeBuffer = index;
                            }
                        }

                        var sqrDist = math.distancesq(casterPosition, positions[i].Value);

                        // Debug.Log($" TrackingTargetEntityInCastRangeSystem {targetEntity.Index}, indexInEntityRangeBuffer = {indexInEntityRangeBuffer}, sqrDist = {sqrDist}, sqrRange = {sqrRange}");
                        //Check if entity is not exist in entityInRangeBuffer and position in range of this caster
                        if (indexInEntityRangeBuffer < 0 && sqrDist <= sqrRange)
                        {
                            var isTargetableEntity = false;
                            // check if entity is a target of this caster
                            foreach (var targetTypeElement in targetTypeBuffer)
                            {
                                switch (targetTypeElement.Value)
                                {
                                    case TargetType.Opponent:
                                        if (teamIds[i].Value != casterTeamId) isTargetableEntity = true;
                                        break;
                                    case TargetType.Self:
                                        if (targetEntity == caster.Value) isTargetableEntity = true;
                                        break;
                                    case TargetType.Ally:
                                        if (teamIds[i].Value == casterTeamId && targetEntity != caster.Value) isTargetableEntity = true;
                                        break;
                                    default:
                                        isTargetableEntity = false;
                                        break;
                                }
                            }

                            if (isTargetableEntity)
                            {
                                Debug.Log($" notifyEntity {targetEntity.Index} in activatedStateAbilityEntity {activatedStateAbilityEntity.Index} range ");
                                entityInRangeBuffer.Add(new EntityInAbilityRangeElement() { Value = targetEntity });
                                var notifyEntity = ecb.CreateNotifyEntity(entityInQueryIndex);
                                ecb.AddComponent(entityInQueryIndex, notifyEntity, new OnInAbilityRange()
                                {
                                    TargetEntity                = targetEntity,
                                    ActivatedStateAbilityEntity = activatedStateAbilityEntity
                                });
                            }
                        }
                        else if (indexInEntityRangeBuffer >= 0 && sqrDist > sqrRange)
                        {
                            Debug.Log($" notifyEntity {targetEntity.Index} out activatedStateAbilityEntity {activatedStateAbilityEntity.Index} range ");
                            entityInRangeBuffer.RemoveAt(indexInEntityRangeBuffer);
                            var notifyEntity = ecb.CreateNotifyEntity(entityInQueryIndex);
                            ecb.AddComponent(entityInQueryIndex, notifyEntity, new OnOutAbilityRange()
                            {
                                TargetEntity                = targetEntity,
                                ActivatedStateAbilityEntity = activatedStateAbilityEntity
                            });
                        }
                    }
                }).ScheduleParallel(jobCombine);
            this.endSimEcbSystem.AddJobHandleForProducer(this.Dependency);
        }
    }
}