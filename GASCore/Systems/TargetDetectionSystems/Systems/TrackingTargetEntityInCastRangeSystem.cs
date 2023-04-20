namespace GASCore.Systems.TargetDetectionSystems.Systems
{
    using DOTSCore.Extension;
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using GASCore.Systems.StatSystems.Components;
    using GASCore.Systems.TargetDetectionSystems.Components;
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
            using var queryBuilder = new EntityQueryBuilder(Allocator.Temp).WithAll<TeamOwnerId, LocalToWorld, StatNameToIndex>();
            this.teamQuery = this.GetEntityQuery(queryBuilder);
        }

        protected override void OnUpdate()
        {
            var ecb        = this.endSimEcbSystem.CreateCommandBuffer().AsParallelWriter();
            var entityList = this.teamQuery.ToEntityListAsync(this.WorldUpdateAllocator, out var getTargetEntityJobHandle);
            var transforms = this.teamQuery.ToComponentDataListAsync<LocalToWorld>(this.WorldUpdateAllocator, out var getPositionJobHandle);
            var jobCombine = JobHandle.CombineDependencies(new NativeArray<JobHandle>(4, Allocator.Temp)
                { [0] = getTargetEntityJobHandle, [1] = getPositionJobHandle, [2] = this.Dependency });

            this.Dependency = Entities.WithBurst().WithAll<NeedToTrackingTargetInCastRange>().WithReadOnly(entityList).WithReadOnly(transforms)
                .ForEach((Entity activatedStateAbilityEntity, int entityInQueryIndex, ref DynamicBuffer<EntityInAbilityRangeElement> entityInRangeBuffer,
                    in CastRangeComponent castRange, in CasterComponent caster) =>
                {
                    var casterPosition = SystemAPI.GetComponent<LocalToWorld>(caster.Value).Position;
                    var sqrRange       = castRange.Value * castRange.Value;

                    for (int i = 0; i < transforms.Length; i++)
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

                        var sqrDist = math.distancesq(casterPosition, transforms[i].Position);

                        // Debug.Log($" TrackingTargetEntityInCastRangeSystem {targetEntity.Index}, indexInEntityRangeBuffer = {indexInEntityRangeBuffer}, sqrDist = {sqrDist}, sqrRange = {sqrRange}");
                        //Check if entity is not exist in entityInRangeBuffer and position in range of this caster
                        if (indexInEntityRangeBuffer < 0 && sqrDist <= sqrRange)
                        {
                            // Debug.Log($" notifyEntity {targetEntity.Index} in activatedStateAbilityEntity {activatedStateAbilityEntity.Index} range ");
                            entityInRangeBuffer.Add(new EntityInAbilityRangeElement() { Value = targetEntity });
                            var notifyEntity = ecb.CreateNotifyEntity(entityInQueryIndex);
                            ecb.AddComponent(entityInQueryIndex, notifyEntity, new OnInAbilityRange()
                            {
                                TargetEntity                = targetEntity,
                                ActivatedStateAbilityEntity = activatedStateAbilityEntity
                            });
                        }
                        else if (indexInEntityRangeBuffer >= 0 && sqrDist > sqrRange)
                        {
                            // Debug.Log($" notifyEntity {targetEntity.Index} out activatedStateAbilityEntity {activatedStateAbilityEntity.Index} range ");
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