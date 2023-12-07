namespace GASCore.Systems.LogicEffectSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using Unity.Burst;
    using Unity.Entities;

    [BurstCompile]
    [WithChangeFilter(typeof(IgnoreKnockBackTag))]
    public partial struct AddIgnoreKnockBackTagJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;
        void Execute([EntityIndexInQuery] int entityInQueryIndex, in IgnoreKnockBackTag ignoreKnockBackTag, in AffectedTargetComponent affectedTargetComponent)
        {
            this.Ecb.AddComponent<IgnoreKnockBackTag>(entityInQueryIndex, affectedTargetComponent.Value);
        }
    }

    [UpdateInGroup(typeof(AbilityLogicEffectGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct AddIgnoreKnockBackTagSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            new AddIgnoreKnockBackTagJob()
            {
                Ecb = ecb
            }.ScheduleParallel();
        }
    }
}