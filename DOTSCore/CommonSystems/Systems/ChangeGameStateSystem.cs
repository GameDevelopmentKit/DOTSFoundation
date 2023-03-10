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
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            new ChangeGameStateJob() { Ecb = ecb }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct ChangeGameStateJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;

        void Execute(Entity gameStateEntity, [EntityIndexInQuery] int entityInQueryIndex, in RequestChangeGameState requestChangeGameState, ref CurrentGameState currentGameState,
            ref PreviousGameState previousGameState)
        {
            if (!requestChangeGameState.IsForce && currentGameState.Value == requestChangeGameState.NextState) return;
            previousGameState.Value = currentGameState.Value;
            currentGameState.Value  = requestChangeGameState.NextState;

            this.Ecb.SetComponentEnabled<RequestChangeGameState>(entityInQueryIndex, gameStateEntity, false);
        }
    }
}