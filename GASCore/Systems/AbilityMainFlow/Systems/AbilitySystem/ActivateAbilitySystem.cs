namespace GASCore.Systems.AbilityMainFlow.Systems.AbilitySystem
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using GASCore.Systems.TargetDetectionSystems.Components;
    using GASCore.Systems.TimelineSystems.Components;
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
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GrantedActivation>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            new ActivateAbilityJob()
            {
                Ecb                           = ecb,
                ChildLookup                   = SystemAPI.GetBufferLookup<Child>(true),
                TriggerByAnotherTriggerLookup = SystemAPI.GetComponentLookup<TriggerByAnotherTrigger>(true)
            }.ScheduleParallel();
        }
        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }

    [WithAll(typeof(GrantedActivation))]
    public partial struct ActivateAbilityJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;

        [ReadOnly] public BufferLookup<Child>                      ChildLookup;
        [ReadOnly] public ComponentLookup<TriggerByAnotherTrigger> TriggerByAnotherTriggerLookup;

        void Execute(Entity abilityEntity, [EntityIndexInQuery] int entityInQueryIndex, in AbilityEffectPoolComponent effectPool, in AbilityTimelinePrefabComponent timelinePrefab,
            in CasterComponent caster, in AbilityId abilityId, in Cooldown cooldown, in CastRangeComponent castRangeComponent)
        {
            // set cooldownTime for ability if available
            if (cooldown.Value > 0)
            {
                this.Ecb.SetComponentEnabled<Duration>(entityInQueryIndex, abilityEntity, true);
                this.Ecb.SetComponent(entityInQueryIndex, abilityEntity, new Duration() { Value = cooldown.Value });
            }

            //update status
            this.Ecb.SetComponentEnabled<GrantedActivation>(entityInQueryIndex, abilityEntity, false);

            //setup activatedStateEntity
            var activatedStateEntity = this.Ecb.CreateEntity(entityInQueryIndex);
            this.Ecb.AppendToBuffer(entityInQueryIndex, abilityEntity, new LinkedEntityGroup() { Value = activatedStateEntity });
            this.Ecb.SetName(entityInQueryIndex, activatedStateEntity, $"ActivatedState_{abilityId.Value}");
            this.Ecb.AddComponent(entityInQueryIndex, activatedStateEntity, caster);
            this.Ecb.AddComponent(entityInQueryIndex, activatedStateEntity, castRangeComponent);
            this.Ecb.AddComponent(entityInQueryIndex, activatedStateEntity, new AbilityOwner() { Value = abilityEntity });
            this.Ecb.AddBuffer<OnDestroyAbilityActionElement>(entityInQueryIndex, activatedStateEntity);
            this.Ecb.AddComponent(entityInQueryIndex, activatedStateEntity, effectPool);

            var linkedEntityGroups = this.Ecb.AddBuffer<LinkedEntityGroup>(entityInQueryIndex, activatedStateEntity);
            linkedEntityGroups.Add(new LinkedEntityGroup() { Value = activatedStateEntity });

            //Instantiate timeline entities in sequence
            if (this.ChildLookup.TryGetBuffer(timelinePrefab.Value, out var children))
            {
                var triggerIndexToAttachedTrigger = new NativeHashMap<int, NativeList<Entity>>(children.Length, Allocator.Temp);
                foreach (var child in children)
                {
                    if (this.TriggerByAnotherTriggerLookup.TryGetComponent(child.Value, out var triggerByAnotherTrigger))
                    {
                        if (!triggerIndexToAttachedTrigger.TryGetValue(triggerByAnotherTrigger.TriggerIndex, out var listEntity))
                        {
                            listEntity = new NativeList<Entity>(Allocator.Temp);
                        }

                        listEntity.Add(child.Value);
                        triggerIndexToAttachedTrigger[triggerByAnotherTrigger.TriggerIndex] = listEntity;
                    }
                }

                for (var index = 0; index < children.Length; index++)
                {
                    var childPrefab = children[index];
                    if (!this.TriggerByAnotherTriggerLookup.HasComponent(childPrefab.Value))
                    {
                        var abilityTimelineAction = this.Ecb.Instantiate(entityInQueryIndex, childPrefab.Value);
                        this.Ecb.AddComponent(entityInQueryIndex, abilityTimelineAction, new ActivatedStateEntityOwner() { Value = activatedStateEntity });
                        this.Ecb.AddComponent(entityInQueryIndex, abilityTimelineAction, caster);
                        this.Ecb.AddBuffer<TargetableElement>(entityInQueryIndex, abilityTimelineAction);
                        this.Ecb.AddBuffer<ExcludeAffectedTargetElement>(entityInQueryIndex, abilityTimelineAction);
                        this.Ecb.RemoveComponent<Parent>(entityInQueryIndex, abilityTimelineAction);
                        linkedEntityGroups.Add(new LinkedEntityGroup() { Value = abilityTimelineAction });

                        if (triggerIndexToAttachedTrigger.TryGetValue(index, out var attachedTriggers))
                        {
                            var waitToTriggerBuffer = this.Ecb.AddBuffer<WaitToTrigger>(entityInQueryIndex, abilityTimelineAction);
                            foreach (var attachedTrigger in attachedTriggers)
                            {
                                waitToTriggerBuffer.Add(new WaitToTrigger() { TriggerEntity = attachedTrigger });
                            }
                        }
                    }
                }

                foreach (var kvPair in triggerIndexToAttachedTrigger)
                {
                    triggerIndexToAttachedTrigger[kvPair.Key].Dispose();
                }

                triggerIndexToAttachedTrigger.Dispose();
            }
        }
    }
}