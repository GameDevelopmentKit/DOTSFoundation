namespace GASCore.Systems.AbilityMainFlow.Systems.AbilitySystem
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using GASCore.Systems.StatSystems.Components;
    using GASCore.Systems.TargetDetectionSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Transforms;

    [UpdateInGroup(typeof(AbilityMainFlowGroup))]
    [UpdateAfter(typeof(ValidateRequestActiveAbilitySystem))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct ActivateAbilitySystem : ISystem
    {
        private StatAspect.Lookup statDataLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state) 
        { 
            state.RequireForUpdate<GrantedActivation>(); 
            statDataLookup = new StatAspect.Lookup(ref state);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            statDataLookup.Update(ref state);

            var ecbSingleton = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            new ActivateAbilityJob() { Ecb = ecb, statDataLookup = this.statDataLookup}.ScheduleParallel();
        }
    }

    [WithAll(typeof(GrantedActivation))]
    public partial struct ActivateAbilityJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;
        [NativeDisableParallelForRestriction] public StatAspect.Lookup statDataLookup;

        void Execute(Entity abilityEntity, [EntityIndexInQuery] int entityInQueryIndex, in AbilityEffectPoolComponent effectPool,
            in DynamicBuffer<AbilityTimelineInitialElement> timelineInitialElements,
            in CasterComponent caster, in Components.AbilityId abilityId, in Cooldown cooldown, in CastRangeComponent castRangeComponent, in DynamicBuffer<AbilityCost> abilityCosts)
        {
            // set cooldownTime for ability if available
            if (cooldown.Value > 0)
            {
                this.Ecb.SetComponentEnabled<Duration>(entityInQueryIndex, abilityEntity, true);
                this.Ecb.SetComponent(entityInQueryIndex, abilityEntity, new Duration() { Value = cooldown.Value });
            }

            //if ability has stat costs, deduct them from caster
            if (abilityCosts.Length > 0)
            {
                StatAspect casterStatData = this.statDataLookup[caster.Value];
                for (int i = 0; i < abilityCosts.Length; i++)
                {
                    var abilityCost = abilityCosts[i];
                    var baseValue = casterStatData.GetBaseValue(abilityCost.Name);
                    casterStatData.SetBaseValue(abilityCost.Name, baseValue - abilityCost.Value);
                }
            }

                //update status
            this.Ecb.SetComponentEnabled<GrantedActivation>(entityInQueryIndex, abilityEntity, false);
            this.Ecb.SetComponentEnabled<ActivatedTag>(entityInQueryIndex, abilityEntity, true);

            //setup activatedStateEntity
            var activatedInstanceEntity = this.Ecb.CreateEntity(entityInQueryIndex);
            this.Ecb.AppendToBuffer(entityInQueryIndex, abilityEntity, new LinkedEntityGroup() { Value = activatedInstanceEntity });
            this.Ecb.SetName(entityInQueryIndex, activatedInstanceEntity, $"ActivatedState_{abilityId.Value}");
            this.Ecb.AddComponent(entityInQueryIndex, activatedInstanceEntity, caster);
            this.Ecb.AddComponent(entityInQueryIndex, activatedInstanceEntity, castRangeComponent);
            this.Ecb.AddComponent(entityInQueryIndex, activatedInstanceEntity, new AbilityOwner() { Value = abilityEntity });
            this.Ecb.AddBuffer<OnDestroyAbilityActionElement>(entityInQueryIndex, activatedInstanceEntity);
            this.Ecb.AddComponent(entityInQueryIndex, activatedInstanceEntity, effectPool);

            var linkedEntityGroups = this.Ecb.AddBuffer<LinkedEntityGroup>(entityInQueryIndex, activatedInstanceEntity);
            linkedEntityGroups.Add(new LinkedEntityGroup() { Value = activatedInstanceEntity });

            //Instantiate timeline entities in sequence
            foreach (var initialElement in timelineInitialElements)
            {
                var abilityTimelineAction = this.Ecb.Instantiate(entityInQueryIndex, initialElement.Prefab);
                this.Ecb.AddComponent(entityInQueryIndex, abilityTimelineAction, new ActivatedStateEntityOwner() { Value = activatedInstanceEntity });
                this.Ecb.AddComponent(entityInQueryIndex, abilityTimelineAction, caster);
                this.Ecb.AddBuffer<TargetableElement>(entityInQueryIndex, abilityTimelineAction);
                this.Ecb.AddBuffer<ExcludeAffectedTargetElement>(entityInQueryIndex, abilityTimelineAction);
                this.Ecb.RemoveComponent<Parent>(entityInQueryIndex, abilityTimelineAction);
                linkedEntityGroups.Add(new LinkedEntityGroup() { Value = abilityTimelineAction });
            }
        }
    }
}