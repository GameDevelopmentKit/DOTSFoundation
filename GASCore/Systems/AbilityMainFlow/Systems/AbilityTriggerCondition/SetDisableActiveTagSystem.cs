namespace GASCore.Systems.AbilityMainFlow.Systems.AbilitySystem
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.TimelineSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityMainFlowGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct SetDisableActiveTagSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new SetDisableActiveTagJob()
            {
                CasterComponentLookup = SystemAPI.GetComponentLookup<CasterComponent>(true)
            }.ScheduleParallel();

            new CheckActiveOnTimeJob().ScheduleParallel();
        }
    }

    [BurstCompile]
    [WithAll(typeof(AbilityId), typeof(ActiveOneTimeTag))]
    [WithNone(typeof(ActivatedTag))]
    public partial struct CheckActiveOnTimeJob : IJobEntity
    {
        void Execute(ref DynamicBuffer<CompletedTriggerElement> completedTriggerBuffer)
        {
            completedTriggerBuffer.Add(new CompletedTriggerElement() { Index = TypeManager.GetTypeIndex<ActiveOneTimeTag>() });
        }
    }

    [BurstCompile]
    [WithAll(typeof(AbilityId), typeof(ActiveOneTimeTag), typeof(ActivatedTag))]
    public partial struct SetDisableActiveTagJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<CasterComponent> CasterComponentLookup;
        void Execute(ref DynamicBuffer<LinkedEntityGroup> linkedEntityGroups, EnabledRefRW<ActivatedTag> activatedTagEnableState)
        {
            for (int i = 0; i < linkedEntityGroups.Length;)
            {
                if (!this.CasterComponentLookup.HasComponent(linkedEntityGroups[i].Value))
                {
                    linkedEntityGroups.RemoveAtSwapBack(i);
                }
                else
                {
                    i++;
                }
            }

            if (linkedEntityGroups.Length <= 1)
            {
                activatedTagEnableState.ValueRW = false;
            }
        }
    }
}