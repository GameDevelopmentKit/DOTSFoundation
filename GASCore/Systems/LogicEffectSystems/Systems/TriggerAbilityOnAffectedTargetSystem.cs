namespace GASCore.Systems.LogicEffectSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.CasterSystems.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityLogicEffectGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct TriggerAbilityOnAffectedTargetSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { state.RequireForUpdate<TriggerAbilityComponent>(); }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            new TriggerAbilityOnAffectedTargetJob()
            {
                Ecb                    = ecb,
                AbilityContainerLookup = SystemAPI.GetBufferLookup<AbilityContainerElement>(true)
            }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct TriggerAbilityOnAffectedTargetJob : IJobEntity
    {
        public            EntityCommandBuffer.ParallelWriter    Ecb;
        [ReadOnly] public BufferLookup<AbilityContainerElement> AbilityContainerLookup;
        void Execute([EntityIndexInQuery] int entityInQueryIndex, in AffectedTargetComponent affectedTarget, in TriggerAbilityComponent triggerAbilityComponent)
        {
            if (this.AbilityContainerLookup.TryGetBuffer(affectedTarget, out var abilityContainer))
            {
                this.Ecb.SetComponentEnabled<RequestActivate>(entityInQueryIndex, abilityContainer[triggerAbilityComponent.Slot].AbilityInstance, true);
            }
        }
    }
}