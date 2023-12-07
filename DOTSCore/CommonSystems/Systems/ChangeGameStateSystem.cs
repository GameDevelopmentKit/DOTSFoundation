namespace DOTSCore.CommonSystems.Systems
{
    using DOTSCore.CommonSystems.Components;
    using Unity.Burst;
    using Unity.Entities;

    [UpdateInGroup(typeof(PresentationSystemGroup), OrderLast = true)]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct ChangeGameStateSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { state.RequireForUpdate<CurrentGameState>(); }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (requestChangeGameState, currentGameState, previousGameState, gameStateEntity) in SystemAPI.Query<RequestChangeGameState, RefRW<CurrentGameState>, RefRW<PreviousGameState>>().WithEntityAccess())
            {
                if (!requestChangeGameState.IsForce && currentGameState.ValueRO.Value == requestChangeGameState.NextState) return;
                previousGameState.ValueRW.Value = currentGameState.ValueRO.Value;
                currentGameState.ValueRW.Value  = requestChangeGameState.NextState;

                SystemAPI.SetComponentEnabled<RequestChangeGameState>(gameStateEntity, false);
            }
        }
    }
}