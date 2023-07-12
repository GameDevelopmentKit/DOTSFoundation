namespace GASCore.Systems.LogicEffectSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.CasterSystems.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using Unity.Burst;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityLogicEffectGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct RequestAddAbilitySystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<AddAbilityElement>().WithNone<RequestAddOrUpgradeAbility>().Build());
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            new RequestAddAbilityJob()
            {
                Ecb = ecb,
            }.ScheduleParallel();
        }
    }

    [BurstCompile]
    [WithNone(typeof(RequestAddOrUpgradeAbility))]
    public partial struct RequestAddAbilityJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;

        void Execute(SourceComponent entity,[EntityIndexInQuery] int entityInQueryIndex, in DynamicBuffer<AddAbilityElement> addAbilityElements)
        {
            var requestAddAbilities = this.Ecb.AddBuffer<RequestAddOrUpgradeAbility>(entityInQueryIndex, entity);

            foreach (var abilityInfo in addAbilityElements)
            {
                requestAddAbilities.Add(new RequestAddOrUpgradeAbility(abilityInfo.AbilityId, abilityInfo.Level));
            }
        }
    }
}