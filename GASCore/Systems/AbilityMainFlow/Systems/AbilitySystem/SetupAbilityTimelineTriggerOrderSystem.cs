namespace GASCore.Systems.AbilityMainFlow.Systems.AbilitySystem
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.TimelineSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Transforms;

    [UpdateInGroup(typeof(AbilityMainFlowGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct SetupAbilityTimelineTriggerOrderSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            using var entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<AbilityId, AbilityTimelinePrefabComponent>().WithNone<AbilityTimelineInitialElement>();
            state.RequireForUpdate(state.GetEntityQuery(entityQuery));
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            new SetupTriggerOrderJob()
            {
                Ecb                           = ecb,
                ChildLookup                   = SystemAPI.GetBufferLookup<Child>(true),
                TriggerByAnotherTriggerLookup = SystemAPI.GetComponentLookup<TriggerByAnotherTrigger>(true)
            }.ScheduleParallel();
        }
    }

    [BurstCompile]
    [WithAll(typeof(AbilityId))]
    [WithNone(typeof(AbilityTimelineInitialElement))]
    public partial struct SetupTriggerOrderJob : IJobEntity
    {
        public            EntityCommandBuffer.ParallelWriter       Ecb;
        [ReadOnly] public BufferLookup<Child>                      ChildLookup;
        [ReadOnly] public ComponentLookup<TriggerByAnotherTrigger> TriggerByAnotherTriggerLookup;
        public void Execute(Entity abilityEntity, [EntityIndexInQuery] int entityInQueryIndex, in AbilityTimelinePrefabComponent timelinePrefab)
        {
            var timelineInitialElements = this.Ecb.AddBuffer<AbilityTimelineInitialElement>(entityInQueryIndex, abilityEntity);
            if (this.ChildLookup.TryGetBuffer(timelinePrefab.Value, out var children))
            {
                var triggerIndexToAttachedTrigger = new NativeHashMap<int, NativeList<Entity>>(children.Length, Allocator.Temp);
                foreach (var child in children)
                {
                    if (!this.TriggerByAnotherTriggerLookup.TryGetComponent(child.Value, out var triggerByAnotherTrigger))
                    {
                        timelineInitialElements.Add(new AbilityTimelineInitialElement() { Prefab = child.Value });
                    }
                    else
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
                    var childPrefab = children[index].Value;
                    if (triggerIndexToAttachedTrigger.TryGetValue(index, out var attachedTriggers))
                    {
                        var waitToTriggerBuffer = this.Ecb.AddBuffer<WaitToTrigger>(entityInQueryIndex, childPrefab);
                        foreach (var attachedTrigger in attachedTriggers)
                        {
                            waitToTriggerBuffer.Add(new WaitToTrigger() { TriggerEntity = attachedTrigger });
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