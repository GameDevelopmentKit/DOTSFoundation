namespace GASCore.Systems.LogicEffectSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using GASCore.Systems.TimelineSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    
    [UpdateInGroup(typeof(AbilityLogicEffectGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct RemoveEffectOnAffectedTargetSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { state.RequireForUpdate<RemoveEffectOnAffectedTarget>(); }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            var setEndTimeTriggerAfterSecondJob = new RemoveEffectOnAffectedTargetJob()
            {
                Ecb                       = ecb,
                LinkedEntityLookup        = SystemAPI.GetBufferLookup<LinkedEntityGroup>(true),
                AbilityEffectIdLookup     = SystemAPI.GetComponentLookup<AbilityEffectId>(true),
                AffectedTargetLookup      = SystemAPI.GetComponentLookup<AffectedTargetComponent>(true),
                CreateAbilityEffectLookup = SystemAPI.GetBufferLookup<CreateAbilityEffectElement>(true),
            };
            setEndTimeTriggerAfterSecondJob.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct RemoveEffectOnAffectedTargetJob : IJobEntity
    {
        public            EntityCommandBuffer.ParallelWriter       Ecb;
        [ReadOnly] public BufferLookup<LinkedEntityGroup>          LinkedEntityLookup;
        [ReadOnly] public ComponentLookup<AbilityEffectId>         AbilityEffectIdLookup;
        [ReadOnly] public ComponentLookup<AffectedTargetComponent> AffectedTargetLookup;
        [ReadOnly] public BufferLookup<CreateAbilityEffectElement> CreateAbilityEffectLookup;
        void Execute([EntityIndexInQuery] int entityInQueryIndex, in ActivatedStateEntityOwner activatedStateEntityOwner, in AffectedTargetComponent affectedTarget,
            in RemoveEffectOnAffectedTarget removeEffectOnAffectedTarget)
        {
            if (!this.LinkedEntityLookup.TryGetBuffer(activatedStateEntityOwner.Value, out var linkedEntityGroups)) return;
            foreach (var linkedEntity in linkedEntityGroups)
            {
                // remove directly the ability effect entity with same effectId and same affected target if exist
                if (this.AbilityEffectIdLookup.TryGetComponent(linkedEntity.Value, out var effectId) && effectId.Value.Equals(removeEffectOnAffectedTarget.EffectId))
                {
                    if (this.AffectedTargetLookup.TryGetComponent(linkedEntity.Value, out var affectedTargetComponent) && affectedTargetComponent.Value.Equals(affectedTarget.Value))
                    {
                        this.Ecb.AddComponent<ForceCleanupTag>(entityInQueryIndex, linkedEntity.Value);
                    }
                }

                if (!removeEffectOnAffectedTarget.IncludeIntendedToCreate) continue;
                //Add exclude affected target to trigger entity that going to create a ability effect with same effect Id,
                //to prevent that ability effect is created for affectedTarget
                if (!this.CreateAbilityEffectLookup.TryGetBuffer(linkedEntity.Value, out var createAbilityEffectElements)) continue;
                foreach (var createAbilityEffectElement in createAbilityEffectElements)
                {
                    if (!createAbilityEffectElement.EffectId.Equals(removeEffectOnAffectedTarget.EffectId)) continue;
                    this.Ecb.AppendToBuffer(entityInQueryIndex, linkedEntity.Value, new ExcludeAffectedTargetElement() { Value = affectedTarget.Value });
                }
            }
        }
    }
}