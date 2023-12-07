namespace GASCore.Systems.TargetDetectionSystems.Systems
{
    using GASCore.Systems.TargetDetectionSystems.Components;
    using GASCore.Systems.TimelineSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;

    [UpdateInGroup(typeof(FindTargetGroup), OrderFirst = true)]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct RecycleFindTargetComponentSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new RecycleFindTargetComponentJob()
            {
                FindTargetLookup = SystemAPI.GetComponentLookup<FindTargetComponent>()
            }.ScheduleParallel();
        }
    }

    [WithDisabled(typeof(FindTargetComponent))]
    [BurstCompile]
    public partial struct RecycleFindTargetComponentJob : IJobEntity
    {
        [NativeDisableParallelForRestriction] public ComponentLookup<FindTargetComponent> FindTargetLookup;
        private void Execute(
            Entity entity,
            in TriggerConditionAmount triggersAmount,
            in DynamicBuffer<CompletedTriggerElement> completedTriggers)
        {
            var findTargetTrigger = TypeManager.GetTypeIndex<FindTargetComponent>().Index;
            foreach (var trigger in completedTriggers)
            {
                // completed, wait for recycle
                if (trigger == findTargetTrigger) return;
            }

            // wait for other triggers
            if (this.FindTargetLookup[entity].WaitForOtherTriggers && completedTriggers.Length < triggersAmount - 1) return;
            this.FindTargetLookup.SetComponentEnabled(entity, true);
        }
    }
}