namespace GASCore.Systems.LogicEffectSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.LogicEffectSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityVisualEffectGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct CountdownTimeSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new CountDownJob()
            {
                DeltaTime      = SystemAPI.Time.DeltaTime,
                DurationLookup = SystemAPI.GetComponentLookup<Duration>()
            }.ScheduleParallel();
        }

        [BurstCompile]
        [WithAll(typeof(Duration))]
        public partial struct CountDownJob : IJobEntity
        {
            public                                       float                     DeltaTime;
            [NativeDisableParallelForRestriction] public ComponentLookup<Duration> DurationLookup;
            void Execute(Entity entity)
            {
                var duration = this.DurationLookup[entity];
                duration.Value              -= DeltaTime;
                this.DurationLookup[entity] =  duration;
                if (duration.Value <= 0) this.DurationLookup.SetComponentEnabled(entity, false);
            }
        }
    }
}