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
        private BufferLookup<LinkedEntityGroup>          linkedEntityLookup;
        private ComponentLookup<AbilityEffectId>         abilityEffectIdLookup;
        private ComponentLookup<AffectedTargetComponent> affectedTargetLookup;
        private BufferLookup<CreateAbilityEffectElement> createAbilityEffectLookup;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            this.linkedEntityLookup        = state.GetBufferLookup<LinkedEntityGroup>(true);
            this.abilityEffectIdLookup     = state.GetComponentLookup<AbilityEffectId>(true);
            this.affectedTargetLookup      = state.GetComponentLookup<AffectedTargetComponent>(true);
            this.createAbilityEffectLookup = state.GetBufferLookup<CreateAbilityEffectElement>(true);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            this.linkedEntityLookup.Update(ref state);
            this.abilityEffectIdLookup.Update(ref state);
            this.affectedTargetLookup.Update(ref state);
            this.createAbilityEffectLookup.Update(ref state);

            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            var setEndTimeTriggerAfterSecondJob = new RemoveEffectOnAffectedTargetJob()
            {
                Ecb                       = ecb,
                AffectedTargetLookup      = this.affectedTargetLookup,
                LinkedEntityLookup        = this.linkedEntityLookup,
                AbilityEffectIdLookup     = this.abilityEffectIdLookup,
                CreateAbilityEffectLookup = this.createAbilityEffectLookup
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
        void Execute([EntityInQueryIndex] int entityInQueryIndex, in ActivatedStateEntityOwner activatedStateEntityOwner, in AffectedTargetComponent affectedTarget,
            in RemoveEffectOnAffectedTarget removeEffectOnAffectedTarget)
        {
            if (this.LinkedEntityLookup.TryGetBuffer(activatedStateEntityOwner.Value, out var linkedEntityGroups))
            {
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

                    if (removeEffectOnAffectedTarget.IncludeIntendedToCreate)
                    {
                        //Add exclude affected target to trigger entity that going to create a ability effect with same effect Id,
                        //to prevent that ability effect is created for affectedTarget
                        if (this.CreateAbilityEffectLookup.TryGetBuffer(linkedEntity.Value, out var createAbilityEffectElements))
                        {
                            foreach (var createAbilityEffectElement in createAbilityEffectElements)
                            {
                                if (createAbilityEffectElement.EffectId.Equals(removeEffectOnAffectedTarget.EffectId))
                                {
                                    this.Ecb.AppendToBuffer(entityInQueryIndex, linkedEntity.Value, new ExcludeAffectedTargetElement() { Value = affectedTarget.Value });
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}