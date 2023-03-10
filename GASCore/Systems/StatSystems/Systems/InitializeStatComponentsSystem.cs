namespace GASCore.Systems.StatSystems.Systems
{
    using DOTSCore.Extension;
    using GASCore.Groups;
    using GASCore.Systems.StatSystems.Components;
    using GASCore.Systems.TimelineSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityLogicEffectGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct InitializeStatComponentsSystem : ISystem
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
            new InitializeStatComponentsJob() { Ecb    = ecb }.ScheduleParallel();
        }
    }

    [BurstCompile]
    [WithNone(typeof(StatNameToIndex))]
    public partial struct InitializeStatComponentsJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;

        void Execute(Entity entity, [EntityIndexInQuery] int entityInQueryIndex, in DynamicBuffer<StatDataElement> statDataElements)
        {
            var statToIndex = new NativeHashMap<FixedString64Bytes, int>(statDataElements.Length, Allocator.Persistent);

            for (var i = 0; i < statDataElements.Length; i++)
            {
                statToIndex.Add(statDataElements[i].StatName, i);
            }

            Ecb.AddComponent(entityInQueryIndex, entity, new StatNameToIndex() { BlobValue = statToIndex.CreateReference() });
            
            Ecb.AddBuffer<OnStatChange>(entityInQueryIndex, entity);
            this.Ecb.SetComponentEnabled<OnStatChange>(entityInQueryIndex, entity, false);
        }
    }
}