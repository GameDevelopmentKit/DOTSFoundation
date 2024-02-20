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
    public partial struct RequestRemoveAbilityFromAffectedTargetSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { state.RequireForUpdate<RemoveAbilityFromAffectedTargetElement>(); }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            new RequestRemoveAbilityFromAffectedTargetJob()
            {
                Ecb = ecb,
                RequestRemoveAbilityLookup = SystemAPI.GetBufferLookup<RequestRemoveAbility>(true)
            }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct RequestRemoveAbilityFromAffectedTargetJob : IJobEntity
    {
        [ReadOnly] public BufferLookup<RequestRemoveAbility> RequestRemoveAbilityLookup;
        public            EntityCommandBuffer.ParallelWriter Ecb;

        void Execute(AffectedTargetComponent entity, [EntityIndexInQuery] int entityInQueryIndex, in DynamicBuffer<RemoveAbilityFromAffectedTargetElement> removeAbilityElements)
        {
            
            if (!this.RequestRemoveAbilityLookup.HasBuffer(entity))
            {
                this.Ecb.AddBuffer<RequestRemoveAbility>(entityInQueryIndex, entity);
            }

            foreach (var abilityInfo in removeAbilityElements)
            {
                this.Ecb.AppendToBuffer(entityInQueryIndex, entity, new RequestRemoveAbility(abilityInfo.AbilityId));
            }
        }
    }
}