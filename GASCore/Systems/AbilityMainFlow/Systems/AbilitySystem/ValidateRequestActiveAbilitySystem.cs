namespace GASCore.Systems.AbilityMainFlow.Systems.AbilitySystem
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using Unity.Burst;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityMainFlowGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct ValidateRequestActiveAbilitySystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            new ValidateAbilityJob() { Ecb = ecb }.ScheduleParallel();
        }
    }

    [WithAll(typeof(RequestActivate), typeof(AbilityId))]
    [WithNone(typeof(GrantedActivation), typeof(Duration))]
    public partial struct ValidateAbilityJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;
        private void Execute(Entity abilityEntity, [EntityInQueryIndex] int entityInQueryIndex)
        {
            this.Ecb.SetComponentEnabled<RequestActivate>(entityInQueryIndex, abilityEntity, false);
            this.Ecb.SetComponentEnabled<GrantedActivation>(entityInQueryIndex, abilityEntity, true);
        }
    }
}