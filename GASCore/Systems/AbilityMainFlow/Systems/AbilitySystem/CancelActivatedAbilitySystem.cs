namespace GASCore.Systems.AbilityMainFlow.Systems.AbilitySystem
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using Unity.Burst;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityCleanupSystemGroup), OrderLast = true)]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct CancelActivatedAbilitySystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { state.RequireForUpdate<RequestCancel>(); }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            new CancelActivatedAbilityJob()
            {
                Ecb = ecb
            }.ScheduleParallel();
        }
    }

    [WithAll(typeof(RequestCancel), typeof(AbilityId))]
    public partial struct CancelActivatedAbilityJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;
        private void Execute(Entity abilityEntity, [EntityIndexInQuery] int entityInQueryIndex ,EnabledRefRW<RequestCancel> requestCancelEnableState, ref DynamicBuffer<LinkedEntityGroup> linkedEntityGroup)
        {
            requestCancelEnableState.ValueRW = false;
            
            for (var index = 0; index < linkedEntityGroup.Length; )
            {
                var linkedEntity = linkedEntityGroup[index];
                if (linkedEntity.Value == abilityEntity)
                {
                    index++;
                    continue;
                }

                if (linkedEntity.Value != Entity.Null)
                {
                    this.Ecb.DestroyEntity(entityInQueryIndex, linkedEntity.Value);
                }

                linkedEntityGroup.RemoveAtSwapBack(index);
            }
        }
    }
}