namespace GASCore.Systems.EntityGeneratorSystems.Systems
{
    using DOTSCore.Extension;
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.EntityGeneratorSystems.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;
    using Random = Unity.Mathematics.Random;

    [UpdateInGroup(typeof(AbilityCommonSystemGroup), OrderFirst = true)]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct SpawnEntitiesSystem : ISystem
    {
        EntityQuery entityQuery;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            this.entityQuery = SystemAPI.QueryBuilder().WithAll<EntitySpawner>().WithNone<EndTimeComponent>().Build();
            state.RequireForUpdate(this.entityQuery);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (this.entityQuery.IsEmpty) return;
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            var random       = new Random((uint)(SystemAPI.Time.ElapsedTime * 100000));

            new SpawnEntitiesJob()
            {
                Ecb             = ecb,
                Random          = random,
                TeamLookup      = SystemAPI.GetComponentLookup<TeamOwnerId>(true)
            }.ScheduleParallel();
        }
    }

    [WithAll(typeof(EntitySpawner))]
    [WithNone(typeof(EndTimeComponent))]
    [WithOptions(EntityQueryOptions.FilterWriteGroup)]
    [BurstCompile]
    public partial struct SpawnEntitiesJob : IJobEntity
    {
        public            EntityCommandBuffer.ParallelWriter Ecb;
        public            Random                             Random;
        [ReadOnly] public ComponentLookup<TeamOwnerId>       TeamLookup;
        private void Execute(
            Entity spawnerEntity,
            [EntityIndexInQuery] int index,
            ref EntitySpawner spawnData,
            in ActivatedStateEntityOwner activatedStateEntityOwner,
            in CasterComponent caster,
            in AbilityEffectId effectId,
            in AffectedTargetComponent affectedTarget,
            in LocalToWorld transform)
        {
            if (this.Random.NextFloat(0f, 1f) > spawnData.SpawnChance) return;

            if (!spawnData.IsSetStartAngle)
            {
                spawnData.CurrentAngle = this.Random.NextFloat(spawnData.StartAngleRange.min, spawnData.StartAngleRange.max);

                spawnData.IsSetStartAngle = true;
            }

            var amount                     = this.Random.NextInt(spawnData.AmountRange.min, spawnData.AmountRange.max);
            var startEntitySpawnerRotation = spawnData.CurrentAngle;
            var startEntitySpawnerPosition = spawnData.CurrentPosition;
            while (amount-- > 0)
            {
                var newEntity = this.Ecb.Instantiate(index, spawnData.EntityPrefab);

                var rotateY = quaternion.RotateY(spawnData.CurrentAngle);
                var rotate  = spawnData.IsLookSpawnerRotation ? math.mul(transform.Rotation, rotateY) : rotateY;

                var position = math.forward(rotate) * spawnData.SpawnerRadius;
                position += math.forward(rotate) * spawnData.CurrentPosition;

                if (spawnData.IsSetChild)
                {
                    this.Ecb.SetParent(index, newEntity, spawnerEntity);
                }
                else
                {
                    this.Ecb.RemoveParent(index, newEntity);
                    position += transform.Position;
                }

                this.Ecb.SetComponent(index, newEntity, LocalTransform.FromPositionRotation(position, rotate));
                spawnData.CurrentPosition += this.Random.NextFloat(spawnData.PositionStepRange.min, spawnData.PositionStepRange.max);
                spawnData.CurrentAngle    += this.Random.NextFloat(spawnData.AngleStepRange.min, spawnData.AngleStepRange.max) * spawnData.Clockwise;

                this.Ecb.AddComponent(index, newEntity, new AbilityEffectId() { Value         = effectId.Value });
                this.Ecb.AddComponent(index, newEntity, new AffectedTargetComponent() { Value = affectedTarget.Value });
                this.Ecb.AddComponent(index, newEntity, caster);

                if (!spawnData.IsDrop)
                {
                    this.Ecb.AddComponent(index, newEntity, activatedStateEntityOwner);
                    this.Ecb.AppendToBuffer(index, activatedStateEntityOwner.Value, new LinkedEntityGroup() { Value = newEntity });
                }
                else
                {
                    this.Ecb.AddComponent(index, newEntity, this.TeamLookup[caster.Value]);
                }
            }

            if (spawnData.IsResetRotationAfterSpawn)
            {
                spawnData.CurrentAngle = startEntitySpawnerRotation;
            }

            spawnData.CurrentPosition = startEntitySpawnerPosition;
        }
    }
}