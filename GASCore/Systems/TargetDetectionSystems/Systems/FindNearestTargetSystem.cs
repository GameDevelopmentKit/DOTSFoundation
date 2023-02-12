namespace GASCore.Systems.TargetDetectionSystems.Systems
{
    using DOTSCore.Extension;
    using GASCore.Groups;
    using GASCore.Services;
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

    [UpdateInGroup(typeof(AbilityTimelineGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial class FindNearestTargetSystem : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem endSimEcbSystem;
        private EntityQuery                            teamQuery;
        private ComponentLookup<LocalToWorld>          translationLookup;
        private ComponentLookup<TagComponent>          tagLookup;

        protected override void OnCreate()
        {
            this.endSimEcbSystem = this.World.GetExistingSystemManaged<EndSimulationEntityCommandBufferSystem>();
            this.teamQuery       = new EntityQueryBuilder(Allocator.Temp).WithAll<StatDataElement,Translation, LocalToWorld>().Build(this);

            this.translationLookup = this.GetComponentLookup<LocalToWorld>(true);
            this.tagLookup         = this.GetComponentLookup<TagComponent>(true);
        }

        protected override void OnUpdate()
        {
            var ecb                    = this.endSimEcbSystem.CreateCommandBuffer().AsParallelWriter();
            var entityList             = this.teamQuery.ToEntityListAsync(this.WorldUpdateAllocator, out var getTargetEntityJobHandle);
            var localTranslationLookup = this.translationLookup.UpdateComponentLookup(this);
            var localTagLookup         = this.tagLookup.UpdateComponentLookup(this);
            this.Dependency = this.Entities.WithBurst().WithChangeFilter<FindNearestTarget>().WithReadOnly(entityList).WithReadOnly(localTranslationLookup).WithReadOnly(localTagLookup)
                .ForEach((Entity triggerEntity, int entityInQueryIndex, ref DynamicBuffer<TargetableElement> targetBuffer, in CasterComponent caster, in FindNearestTarget findNearestTarget) =>
                {
                    var casterPosition = localTranslationLookup[caster.Value].Position;

                    var minDist           = float.MaxValue;
                    var targetEntityIndex = -1;

                    for (var index = 0; index < entityList.Length; index++)
                    {
                        var tempTargetEntity = entityList[index];
                        if (findNearestTarget.IsIncludedTag)
                        {
                            if (!localTagLookup.HasComponent(tempTargetEntity)) continue;
                            if (!findNearestTarget.TargetTag.Equals(localTagLookup[tempTargetEntity].Value)) continue;
                        }

                        var distancesq = math.distancesq(casterPosition, localTranslationLookup[tempTargetEntity].Position);

                        if (minDist > distancesq)
                        {
                            minDist           = distancesq;
                            targetEntityIndex = index;
                        }
                    }

                    if (targetEntityIndex == -1) return;
                    targetBuffer.Add(new TargetableElement() { Value = entityList[targetEntityIndex] });
                    ecb.MarkTriggerConditionComplete<FindNearestTarget>(triggerEntity, entityInQueryIndex);
                }).ScheduleParallel(JobHandle.CombineDependencies(getTargetEntityJobHandle, this.Dependency));
        }
    }
}