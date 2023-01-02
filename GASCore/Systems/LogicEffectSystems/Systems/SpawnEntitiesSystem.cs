namespace GASCore.Systems.LogicEffectSystems.Systems
{
    using DOTSCore.Extension;
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.CommonSystems.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Transforms;
    using UnityEngine;
    using Random = Unity.Mathematics.Random;

    [UpdateInGroup(typeof(AbilityLogicEffectGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct SpawnEntitiesSystem : ISystem
    {
        private EntityQuery                  spawnerEntityQuery;
        private ComponentLookup<TeamOwnerId> teamLookup;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            using var queryBuilder = new EntityQueryBuilder(Allocator.Temp).WithAll<EntitySpawner>().WithNone<EndTimeComponent>().WithOptions(EntityQueryOptions.FilterWriteGroup);
            this.spawnerEntityQuery = state.GetEntityQuery(queryBuilder);
            this.teamLookup         = state.GetComponentLookup<TeamOwnerId>(true);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            this.teamLookup.Update(ref state);
            var spawnJob = new SpawnEntitiesJob()
            {
                Ecb = ecb,
                TeamLookup = teamLookup
            };
            spawnJob.ScheduleParallel(spawnerEntityQuery);
        }
    }

    [BurstCompile]
    public partial struct SpawnEntitiesJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;

        [ReadOnly] public ComponentLookup<TeamOwnerId> TeamLookup;

        void Execute([EntityInQueryIndex] int entityInQueryIndex, in EntitySpawner entitySpawner, in ActivatedStateEntityOwner activatedStateEntityOwner, in CasterComponent caster,
            in AbilityEffectId effectId, in AffectedTargetComponent affectedTargetComponent)
        {
            var rnd    = Random.CreateFromIndex((uint)entityInQueryIndex);
            var amount = entitySpawner.IsRandomAmount ? rnd.NextInt(1, entitySpawner.MaxAmount) : entitySpawner.MaxAmount;

            Debug.Log($"SpawnEntitiesJob {entitySpawner.EntityPrefab}-{amount}");

            for (int i = 0; i < amount; i++)
            {
                var entityInstance = this.Ecb.Instantiate(entityInQueryIndex, entitySpawner.EntityPrefab);
                this.Ecb.RemoveParent(entityInQueryIndex, entityInstance);

                this.Ecb.AddComponent(entityInQueryIndex, entityInstance, new AbilityEffectId() { Value         = effectId.Value });
                this.Ecb.AddComponent(entityInQueryIndex, entityInstance, new AffectedTargetComponent() { Value = affectedTargetComponent.Value });
                this.Ecb.AddComponent(entityInQueryIndex, entityInstance, caster);

                this.Ecb.AddComponent(entityInQueryIndex, entityInstance, this.TeamLookup[caster.Value]);

                if (entitySpawner.IsDrop) continue;
                this.Ecb.AddComponent(entityInQueryIndex, entityInstance, activatedStateEntityOwner);
                this.Ecb.AppendToBuffer(entityInQueryIndex, activatedStateEntityOwner.Value, new LinkedEntityGroup() { Value = entityInstance });
            }
        }
    }
}