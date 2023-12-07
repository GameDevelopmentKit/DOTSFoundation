namespace GASCore.Systems.AbilityMainFlow.Systems.AbilitySystem
{
    using DOTSCore.Extension;
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityMainFlowGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct CreateAbilityEffectPoolSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            using var entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<AbilityId, AbilityEffectElement>().WithNone<AbilityEffectPoolComponent>();
            state.RequireForUpdate(state.GetEntityQuery(entityQuery));
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            new CreateAbilityEffectPoolJob()
            {
                Ecb                   = ecb,
                AbilityEffectIdLookup = SystemAPI.GetComponentLookup<AbilityEffectId>(true)
            }.ScheduleParallel();
        }
    }

    [BurstCompile]
    [WithAll(typeof(AbilityId))]
    [WithNone(typeof(AbilityEffectPoolComponent))]
    public partial struct CreateAbilityEffectPoolJob : IJobEntity
    {
        public            EntityCommandBuffer.ParallelWriter Ecb;
        [ReadOnly] public ComponentLookup<AbilityEffectId>   AbilityEffectIdLookup;
        public void Execute(Entity abilityEntity, [EntityIndexInQuery] int entityInQueryIndex, in DynamicBuffer<AbilityEffectElement> effectPoolBuffer)
        {
            var effectIdToEffectPrefab = new NativeHashMap<FixedString64Bytes, Entity>(effectPoolBuffer.Length, Allocator.Persistent);

            foreach (var effectPoolComponent in effectPoolBuffer)
            {
                var effectPrefab = effectPoolComponent.EffectPrefab;
                effectIdToEffectPrefab.Add(this.AbilityEffectIdLookup[effectPrefab].Value, effectPrefab);
            }

            this.Ecb.AddComponent(entityInQueryIndex, abilityEntity, new AbilityEffectPoolComponent() { BlobValue = effectIdToEffectPrefab.CreateReference() });
        }
    }
}