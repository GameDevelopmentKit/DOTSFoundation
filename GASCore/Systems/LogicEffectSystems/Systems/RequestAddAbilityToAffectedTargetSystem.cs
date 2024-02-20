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
    public partial struct RequestAddAbilityToAffectedTargetSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<AddAbilityToAffectedTargetElement>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            new RequestAddAbilityToAffectedTargetJob()
            {
                Ecb = ecb,
                RequestAddAbilityLookup = SystemAPI.GetBufferLookup<RequestAddOrUpgradeAbility>(true)
            }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct RequestAddAbilityToAffectedTargetJob : IJobEntity
    {
        public            EntityCommandBuffer.ParallelWriter       Ecb;
        [ReadOnly] public BufferLookup<RequestAddOrUpgradeAbility> RequestAddAbilityLookup;

        void Execute(AffectedTargetComponent entity,[EntityIndexInQuery] int entityInQueryIndex, in DynamicBuffer<AddAbilityToAffectedTargetElement> addAbilityElements)
        {
            if (!this.RequestAddAbilityLookup.HasBuffer(entity))
            {
                this.Ecb.AddBuffer<RequestAddOrUpgradeAbility>(entityInQueryIndex, entity);
            }

            foreach (var abilityInfo in addAbilityElements)
            {
                this.Ecb.AppendToBuffer(entityInQueryIndex, entity, new RequestAddOrUpgradeAbility(abilityInfo.AbilityId, abilityInfo.Level));
            }
        }
    }
}