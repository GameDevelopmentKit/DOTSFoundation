namespace GASCore.Systems.AbilityMainFlow.Systems.AbilityTriggerCondition
{
    using GASCore.Groups;
    using GASCore.Services;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using GASCore.Systems.TimelineSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityMainFlowGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct AutoRequestActiveAbilitySystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { }
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            new AutoTriggerAbilityAfterCooldownJob() { Ecb = ecb }.ScheduleParallel();
            new AutoTriggerAbilityOnStartJob() { Ecb       = ecb }.ScheduleParallel();

            new AutoRequestActiveAbilityJob()
            {
                Ecb                  = ecb,
                RecycleTriggerLookup = SystemAPI.GetComponentLookup<RecycleTriggerEntityTag>(true)
            }.ScheduleParallel();
        }
        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }

    [BurstCompile]
    [WithAll(typeof(AbilityId), typeof(CompletedAllTriggerConditionTag), typeof(AutoActiveTag))]
    [WithNone(typeof(RequestActivate))]
    public partial struct AutoRequestActiveAbilityJob : IJobEntity
    {
        public            EntityCommandBuffer.ParallelWriter       Ecb;
        [ReadOnly] public ComponentLookup<RecycleTriggerEntityTag> RecycleTriggerLookup;
        void Execute(Entity abilityEntity, [EntityIndexInQuery] int entityInQueryIndex, in AbilityId abilityId)
        {
            // Debug.Log($"AutoRequestActiveAbilityJob ability {abilityId.Value}");
            if (!this.RecycleTriggerLookup.HasComponent(abilityEntity))
            {
                this.Ecb.SetComponentEnabled<AutoActiveTag>(entityInQueryIndex, abilityEntity, false);
            }

            this.Ecb.SetComponentEnabled<RequestActivate>(entityInQueryIndex, abilityEntity, true);
        }
    }

    [BurstCompile]
    [WithAll(typeof(AbilityId), typeof(Cooldown), typeof(AutoActiveAfterCooldownTag))]
    [WithNone(typeof(Duration))]
    public partial struct AutoTriggerAbilityAfterCooldownJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;

        void Execute(Entity abilityEntity, [EntityIndexInQuery] int entityInQueryIndex)
        {
            // Debug.Log($"AutoTriggerAbilityAfterCooldownJob ability Index {abilityEntity.Index}");
            this.Ecb.MarkTriggerConditionComplete<AutoActiveAfterCooldownTag>(abilityEntity, entityInQueryIndex);
        }
    }

    [BurstCompile]
    [WithAll(typeof(AbilityId), typeof(AutoActiveOnStartTag))]
    [WithChangeFilter(typeof(AutoActiveOnStartTag))]
    public partial struct AutoTriggerAbilityOnStartJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;

        void Execute(Entity abilityEntity, [EntityIndexInQuery] int entityInQueryIndex)
        {
            // Debug.Log($"AutoTriggerAbilityOnStartJob ability Index {abilityEntity.Index}");
            this.Ecb.MarkTriggerConditionComplete<AutoActiveOnStartTag>(abilityEntity, entityInQueryIndex);
        }
    }
}