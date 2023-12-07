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

    [UpdateInGroup(typeof(AbilityMainFlowGroup))]
    [UpdateBefore(typeof(InstantiateAbilityEffectFromPoolSystem))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct TriggerAttachedTriggerSystem : ISystem
    {
        EntityQuery entityQuery;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            this.entityQuery = SystemAPI.QueryBuilder().WithAll<WaitToTrigger, CompletedAllTriggerConditionTag>().Build();
            state.RequireForUpdate(this.entityQuery);
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if(this.entityQuery.IsEmpty) return;
            var ecbSingleton = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();

            var setEndTimeTriggerAfterSecondJob = new TriggerAttachedTriggerJob()
            {
                Ecb           = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
                TriggerLookup = SystemAPI.GetComponentLookup<TriggerByAnotherTrigger>(true)
            };
            setEndTimeTriggerAfterSecondJob.ScheduleParallel();
        }
    }

    [BurstCompile]
    [WithAll(typeof(CompletedAllTriggerConditionTag))]
    public partial struct TriggerAttachedTriggerJob : IJobEntity
    {
        public            EntityCommandBuffer.ParallelWriter       Ecb;
        [ReadOnly] public ComponentLookup<TriggerByAnotherTrigger> TriggerLookup;
        void Execute([EntityIndexInQuery] int entityInQueryIndex, in DynamicBuffer<WaitToTrigger> waitToTriggerBuffer, in ActivatedStateEntityOwner activatedStateEntity,
            in CasterComponent caster, in DynamicBuffer<TargetableElement> targetBuffer, in DynamicBuffer<ExcludeAffectedTargetElement> excludeAffectedTargetBuffer)
        {
            foreach (var waitToTrigger in waitToTriggerBuffer)
            {
                var abilityTimelineAction = this.Ecb.Instantiate(entityInQueryIndex, waitToTrigger.TriggerEntity);
                this.Ecb.AddComponent(entityInQueryIndex, abilityTimelineAction, new ActivatedStateEntityOwner() { Value = activatedStateEntity.Value });
                this.Ecb.AddComponent(entityInQueryIndex, abilityTimelineAction, caster);
                this.Ecb.RemoveComponent<Parent>(entityInQueryIndex, abilityTimelineAction);
                this.Ecb.AppendToBuffer(entityInQueryIndex, activatedStateEntity.Value, new LinkedEntityGroup() { Value = abilityTimelineAction });

                var targetBufferClone                = this.Ecb.AddBuffer<TargetableElement>(entityInQueryIndex, abilityTimelineAction);
                var triggerByAnother                 = this.TriggerLookup[waitToTrigger.TriggerEntity];
                var excludeAffectedTargetBufferClone = this.Ecb.AddBuffer<ExcludeAffectedTargetElement>(entityInQueryIndex, abilityTimelineAction);
                if (!triggerByAnother.IsCloneTarget) continue;
                foreach (var targetType in targetBuffer)
                {
                    targetBufferClone.Add(targetType);
                }
                
                foreach (var excludeAffectedTargetElement in excludeAffectedTargetBuffer)
                {
                    excludeAffectedTargetBufferClone.Add(excludeAffectedTargetElement);
                }

            }
        }
    }
}