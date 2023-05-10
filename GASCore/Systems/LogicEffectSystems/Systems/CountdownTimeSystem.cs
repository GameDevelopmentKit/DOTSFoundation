namespace GASCore.Systems.LogicEffectSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.LogicEffectSystems.Components;
    using Unity.Burst;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityLogicEffectGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct CountdownTimeSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Duration>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            foreach (var (duration, entity) in SystemAPI.Query<RefRW<Duration>>().WithEntityAccess())
            {
                duration.ValueRW.Value -= deltaTime;
                if (duration.ValueRO.Value <= 0) SystemAPI.SetComponentEnabled<Duration>(entity, false);
            }
        }
    }
}