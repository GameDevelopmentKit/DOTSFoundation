namespace GASCore.Systems.TimelineSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.TimelineSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityTimelineGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct TrackingTriggerConditionProgressSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            state.Dependency = new TrackingTriggerConditionProgressJob()
            {
                Ecb = ecb,
            }.ScheduleParallel(state.Dependency);
        }
    }

    [WithNone(typeof(CompletedAllTriggerConditionTag))]
    [WithChangeFilter(typeof(CompletedTriggerElement))]
    public partial struct TrackingTriggerConditionProgressJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;
        void Execute(Entity entity, [EntityInQueryIndex] int entityInQueryIndex, ref DynamicBuffer<CompletedTriggerElement> completedTriggerBuffer, in TriggerConditionAmount conditionAmount)
        {
            if (conditionAmount.Value == 0 || (conditionAmount.Value == 1 && completedTriggerBuffer.Length == 1))
            {
                MarkCompletedAllTriggerCondition(entity, entityInQueryIndex);
                return;
            }

            var conditionHashset = new NativeHashSet<int>(conditionAmount.Value, Allocator.Temp);

            for (var index = 0; index < completedTriggerBuffer.Length;)
            {
                var completedTriggerIndex = completedTriggerBuffer[index];
                if (!conditionHashset.Add(completedTriggerIndex.Index))
                {
                    completedTriggerBuffer.RemoveAtSwapBack(index);
                }
                else
                {
                    index++;
                }
            }

            if (conditionHashset.Count == conditionAmount.Value)
            {
                MarkCompletedAllTriggerCondition(entity, entityInQueryIndex);
            }
        }

        void MarkCompletedAllTriggerCondition(Entity entity, int entityInQueryIndex)
        {
            this.Ecb.AddComponent<CompletedAllTriggerConditionTag>(entityInQueryIndex, entity);
            this.Ecb.SetComponentEnabled<CompletedAllTriggerConditionTag>(entityInQueryIndex, entity, true);
            this.Ecb.SetComponentEnabled<InTriggerConditionResolveProcessTag>(entityInQueryIndex, entity, false);
        }
    }
}