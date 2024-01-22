namespace GASCore.Systems.AbilityMainFlow.Systems.AbilitySystem
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using GASCore.Systems.LogicEffectSystems.Systems;
    using GASCore.Systems.TimelineSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityCleanupSystemGroup))]
    [UpdateBefore(typeof(DestroyTargetSystem))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct CleanupUnusedAbilityEntitiesSystem : ISystem
    {
        private EntityQuery                                 forceCleanupEntityQuery;
        private EntityQuery                                 normalCleanupEntityQuery;
        private BufferLookup<OnDestroyAbilityActionElement> onDestroyAbilityActionElementLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            using var queryBuilder = new EntityQueryBuilder(Allocator.Temp).WithAll<ActivatedStateEntityOwner, ForceCleanupTag>();
            this.forceCleanupEntityQuery = state.GetEntityQuery(queryBuilder);

            queryBuilder.Reset();
            queryBuilder.WithAll<ActivatedStateEntityOwner>().WithNone<InTriggerConditionResolveProcessTag, EndTimeComponent, Duration, IgnoreCleanupTag, ForceCleanupTag>();
            this.normalCleanupEntityQuery            = state.GetEntityQuery(queryBuilder);
            this.onDestroyAbilityActionElementLookup = state.GetBufferLookup<OnDestroyAbilityActionElement>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            this.onDestroyAbilityActionElementLookup.Update(ref state);
            var ecbSingleton       = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
            var ecb                = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            new CleanupAbilityActionEntitiesJob()
            {
                Ecb = ecb, OnDestroyAbilityActionElementLookup = this.onDestroyAbilityActionElementLookup,
            }.ScheduleParallel(normalCleanupEntityQuery);

            new CleanupAbilityActionEntitiesJob()
            {
                Ecb               = ecb, OnDestroyAbilityActionElementLookup = this.onDestroyAbilityActionElementLookup,
            }.ScheduleParallel(forceCleanupEntityQuery);

            new CleanupActivatedStateAbilityEntitiesJob() { Ecb = ecb }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct CleanupAbilityActionEntitiesJob : IJobEntity
    {
        public            EntityCommandBuffer.ParallelWriter          Ecb;
        [ReadOnly] public BufferLookup<OnDestroyAbilityActionElement> OnDestroyAbilityActionElementLookup;
        [BurstCompile]
        void Execute(Entity abilityActionEntity, [EntityIndexInQuery] int entityInQueryIndex, in ActivatedStateEntityOwner activatedStateEntityOwner)
        {
            if (this.OnDestroyAbilityActionElementLookup.HasBuffer(activatedStateEntityOwner.Value))
                this.Ecb.AppendToBuffer(0, activatedStateEntityOwner.Value, new OnDestroyAbilityActionElement() { AbilityActionEntity = abilityActionEntity });

            this.Ecb.RemoveComponent<ActivatedStateEntityOwner>(entityInQueryIndex, abilityActionEntity);
            this.Ecb.DestroyEntity(entityInQueryIndex, abilityActionEntity);
        }
    }

    [BurstCompile]
    [WithAll(typeof(AbilityEffectPoolComponent))]
    [WithChangeFilter(typeof(OnDestroyAbilityActionElement))]
    public partial struct CleanupActivatedStateAbilityEntitiesJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;

        void Execute(Entity abilityActivatedStateEntity, [EntityIndexInQuery] int entityInQueryIndex, ref DynamicBuffer<LinkedEntityGroup> linkedEntityGroups,
            ref DynamicBuffer<OnDestroyAbilityActionElement> onDestroyAbilityActionBuffer)
        {
            foreach (var onDestroyAbilityActionElement in onDestroyAbilityActionBuffer)
            {
                for (var index = 0; index < linkedEntityGroups.Length; index++)
                {
                    var linkedEntityGroup = linkedEntityGroups[index];

                    if (!linkedEntityGroup.Value.Equals(onDestroyAbilityActionElement.AbilityActionEntity)) continue;
                    linkedEntityGroups.RemoveAtSwapBack(index);

                    break;
                }
            }

            if (linkedEntityGroups.Length <= 1)
            {
                this.Ecb.DestroyEntity(entityInQueryIndex, abilityActivatedStateEntity);
            }

            onDestroyAbilityActionBuffer.Clear();
        }
    }
}