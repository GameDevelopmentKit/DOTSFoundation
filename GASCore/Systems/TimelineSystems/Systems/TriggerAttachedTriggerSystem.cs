namespace GASCore.Systems.TimelineSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Services;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using GASCore.Systems.TargetDetectionSystems.Components;
    using GASCore.Systems.TimelineSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Transforms;
    using UnityEngine;

    [UpdateInGroup(typeof(AbilityTimelineGroup))]
    [UpdateBefore(typeof(InstantiateAbilityEffectFromPoolSystem))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct TriggerAttachedTriggerSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            var setEndTimeTriggerAfterSecondJob = new TriggerAttachedTriggerJob()
            {
                Ecb                    = ecb
            };
            setEndTimeTriggerAfterSecondJob.ScheduleParallel();
        }
    }

    [BurstCompile]
    [WithAll(typeof(CompletedAllTriggerConditionTag))]
    public partial struct TriggerAttachedTriggerJob : IJobEntity
    {
        public            EntityCommandBuffer.ParallelWriter     Ecb;
        void Execute([EntityInQueryIndex] int entityInQueryIndex, in DynamicBuffer<WaitToTrigger> waitToTriggerBuffer, in ActivatedStateEntityOwner activatedStateEntity,
            in CasterComponent caster, in DynamicBuffer<TargetableElement> targetBuffer, in DynamicBuffer<ExcludeAffectedTargetElement> excludeAffectedTargetBuffer)
        {
            foreach (var waitToTrigger in waitToTriggerBuffer)
            {
                var abilityTimelineAction = this.Ecb.Instantiate(entityInQueryIndex, waitToTrigger.TriggerEntity);
                this.Ecb.AddComponent(entityInQueryIndex, abilityTimelineAction, new ActivatedStateEntityOwner() { Value = activatedStateEntity.Value });
                this.Ecb.AddComponent(entityInQueryIndex, abilityTimelineAction, caster);
                this.Ecb.RemoveComponent<Parent>(entityInQueryIndex, abilityTimelineAction);
                this.Ecb.AppendToBuffer(entityInQueryIndex, activatedStateEntity.Value, new LinkedEntityGroup() { Value = abilityTimelineAction });
                this.Ecb.MarkTriggerConditionComplete<TriggerByAnotherTrigger>(abilityTimelineAction, entityInQueryIndex);

                var targetBufferClone = this.Ecb.AddBuffer<TargetableElement>(entityInQueryIndex, abilityTimelineAction);
                foreach (var targetType in targetBuffer)
                {
                    targetBufferClone.Add(targetType);
                }

                var excludeAffectedTargetBufferClone = this.Ecb.AddBuffer<ExcludeAffectedTargetElement>(entityInQueryIndex, abilityTimelineAction);
                foreach (var excludeAffectedTargetElement in excludeAffectedTargetBuffer)
                {
                    excludeAffectedTargetBufferClone.Add(excludeAffectedTargetElement);
                }
            }
        }
    }
}