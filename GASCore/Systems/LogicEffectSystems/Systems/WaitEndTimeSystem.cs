namespace GASCore.Systems.LogicEffectSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.LogicEffectSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityCleanupSystemGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct WaitEndTimeSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new WaitEndTimeJob()
            {
                EndTimeLookup      = SystemAPI.GetComponentLookup<EndTimeComponent>(),
                CurrentElapsedTime = SystemAPI.Time.ElapsedTime
            }.ScheduleParallel();
        }
    }

    [BurstCompile]
    [WithAll(typeof(EndTimeComponent))]
    public partial struct WaitEndTimeJob : IJobEntity
    {
        public                                       double                            CurrentElapsedTime;
        [NativeDisableParallelForRestriction] public ComponentLookup<EndTimeComponent> EndTimeLookup;
        void Execute(Entity entity)
        {
            var endTimeComponent = this.EndTimeLookup[entity];
            if (endTimeComponent.NextEndTimeValue <= 0)
            {
                endTimeComponent.NextEndTimeValue = this.CurrentElapsedTime + endTimeComponent.AmountTime;
                this.EndTimeLookup[entity]        = endTimeComponent;
            }

            if (this.CurrentElapsedTime >= endTimeComponent.NextEndTimeValue)
            {
                endTimeComponent.NextEndTimeValue = -1;
                this.EndTimeLookup[entity]        = endTimeComponent;
                this.EndTimeLookup.SetComponentEnabled(entity, false);
            }

        }
    }
}