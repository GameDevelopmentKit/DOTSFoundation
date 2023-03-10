namespace GASCore.Systems.AbilityMainFlow.Systems.AbilitySystem
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityMainFlowGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct ValidateRequestActiveAbilitySystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            using var entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<RequestActivate,AbilityId>().WithNone<GrantedActivation,Duration>();
            state.RequireForUpdate(state.GetEntityQuery(entityQuery));
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            new ValidateAbilityJob() { Ecb = ecb }.ScheduleParallel();
        }
    }

    [WithAll(typeof(RequestActivate), typeof(AbilityId))]
    [WithNone(typeof(GrantedActivation), typeof(Duration))]
    public partial struct ValidateAbilityJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;
        private void Execute(Entity abilityEntity, [EntityIndexInQuery] int entityInQueryIndex)
        {
            this.Ecb.SetComponentEnabled<RequestActivate>(entityInQueryIndex, abilityEntity, false);
            this.Ecb.SetComponentEnabled<GrantedActivation>(entityInQueryIndex, abilityEntity, true);
        }
    }
}