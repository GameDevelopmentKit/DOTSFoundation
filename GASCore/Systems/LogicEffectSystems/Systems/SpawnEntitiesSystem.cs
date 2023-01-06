namespace GASCore.Systems.LogicEffectSystems.Systems
{
    using System;
    using DOTSCore.Extension;
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.CommonSystems.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Unity.Entities;
    using Unity.Mathematics;
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
        private ComponentLookup<Rotation>    rotationLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            using var queryBuilder = new EntityQueryBuilder(Allocator.Temp).WithAll<EntitySpawner>().WithNone<EndTimeComponent>().WithOptions(EntityQueryOptions.FilterWriteGroup);
            this.spawnerEntityQuery = state.GetEntityQuery(queryBuilder);
            this.teamLookup         = state.GetComponentLookup<TeamOwnerId>(true);
            this.rotationLookup     = state.GetComponentLookup<Rotation>(true);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            this.teamLookup.Update(ref state);
            this.rotationLookup.Update(ref state);

            var spawnJob = new SpawnEntitiesJob()
            {
                Ecb            = ecb,
                TeamLookup     = this.teamLookup,
                RotationLookup = this.rotationLookup
            };
            spawnJob.ScheduleParallel(spawnerEntityQuery);
        }
    }

    [BurstCompile]
    public partial struct SpawnEntitiesJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;

        [ReadOnly] public ComponentLookup<TeamOwnerId> TeamLookup;
        [ReadOnly] public ComponentLookup<Rotation>    RotationLookup;

        [NativeSetThreadIndex] private int threadId;

        void Execute([EntityInQueryIndex] int index,
            ref EntitySpawner spawnData,
            in ActivatedStateEntityOwner activatedStateEntityOwner,
            in CasterComponent caster,
            in AbilityEffectId effectId,
            in AffectedTargetComponent affectedTarget)
        {
            var rnd = Random.CreateFromIndex((uint)this.threadId);

            if (spawnData.Clockwise == 0)
            { 
                spawnData.CurrentAngle = rnd.NextFloat(spawnData.StartAngleRange.min, spawnData.StartAngleRange.max);
                spawnData.Clockwise    = rnd.NextBool() ? 1 : -1;
            }

            var amount = rnd.NextInt(spawnData.AmountRange.min, spawnData.AmountRange.max);

            while (amount-- > 0)
            {
                var entity = this.Ecb.Instantiate(index, spawnData.EntityPrefab);
                this.Ecb.RemoveParent(index, entity);

                this.Ecb.SetComponent(index, entity, new Rotation { Value = math.mul(this.RotationLookup[caster.Value].Value,quaternion.RotateY(spawnData.CurrentAngle)) });

                this.Ecb.AddComponent(index, entity, new AbilityEffectId() { Value         = effectId.Value });
                this.Ecb.AddComponent(index, entity, new AffectedTargetComponent() { Value = affectedTarget.Value });
                this.Ecb.AddComponent(index, entity, caster);

                this.Ecb.AddComponent(index, entity, this.TeamLookup[caster.Value]);

                if (spawnData.IsDrop) continue;
                this.Ecb.AddComponent(index, entity, activatedStateEntityOwner);
                this.Ecb.AppendToBuffer(index, activatedStateEntityOwner.Value, new LinkedEntityGroup() { Value = entity });
                
                spawnData.CurrentAngle += rnd.NextFloat(spawnData.AngleStepRange.min, spawnData.AngleStepRange.max) * spawnData.Clockwise;
            }
        }
    }
}