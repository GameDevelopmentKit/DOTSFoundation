namespace GASCore.Systems.StatSystems.Systems
{
    using DOTSCore.Extension;
    using GASCore.Groups;
    using GASCore.Systems.StatSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityLogicEffectGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct AddMapStatNameToIndexSystem : ISystem
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
            new AddMapStatNameToIndexJob() { Ecb = ecb }.ScheduleParallel();
        }
    }

    [BurstCompile]
    [WithNone(typeof(StatNameToIndex))]
    public partial struct AddMapStatNameToIndexJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;

        void Execute(Entity entity, [EntityInQueryIndex] int entityInQueryIndex, in DynamicBuffer<StatDataElement> statDataElements)
        {
            var statToIndex = new NativeHashMap<FixedString64Bytes, int>(statDataElements.Length, Allocator.Persistent);

            for (var i = 0; i < statDataElements.Length; i++)
            {
                statToIndex.Add(statDataElements[i].StatName, i);
            }

            Ecb.AddComponent(entityInQueryIndex, entity, new StatNameToIndex() { BlobValue = statToIndex.CreateReference() });
        }
    }
}