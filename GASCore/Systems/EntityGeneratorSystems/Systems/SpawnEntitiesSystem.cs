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

    [UpdateInGroup(typeof(AbilityLogicEffectGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct SpawnEntitiesSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { state.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<EntitySpawner>().WithNone<EndTimeComponent>().Build()); }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            var random       = new Random((uint)(SystemAPI.Time.ElapsedTime * 100000));

            new SpawnEntitiesJob()
            {
                Ecb        = ecb,
                Random     = random,
                TeamLookup = SystemAPI.GetComponentLookup<TeamOwnerId>(true),
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
            in LocalToWorld spawnerTransform)
        {
            if (this.Random.NextFloat(0f, 1f) > spawnData.SpawnChance) return;

            if (spawnData.Clockwise == 0)
            {
                spawnData.CurrentAngle = this.Random.NextFloat(spawnData.StartAngleRange.min, spawnData.StartAngleRange.max);
                spawnData.Clockwise    = this.Random.NextBool() ? 1 : -1;
            }

            var amount                     = this.Random.NextInt(spawnData.AmountRange.min, spawnData.AmountRange.max);
            var startEntitySpawnerRotation = spawnData.CurrentAngle;
            var startEntitySpawnerPosition = spawnData.CurrentPosition;
            while (amount-- > 0)
            {
                var newEntity = this.Ecb.Instantiate(index, spawnData.EntityPrefab);
                this.Ecb.RemoveParent(index, newEntity);

                math.sincos(spawnData.CurrentAngle, out var sinA, out var cosA);
                var position = new float3(sinA, 0.0f, cosA) * spawnData.SpawnerRadius;
                if (spawnData.IsSetChild)
                    this.Ecb.SetParent(index, newEntity, spawnerEntity);
                else
                    position += spawnerTransform.Position;


                var rotateY = quaternion.RotateY(spawnData.CurrentAngle);
                var rotate  = spawnData.IsLookSpawnerRotation ? math.mul(spawnerTransform.Rotation, rotateY) : rotateY;
                position += math.forward(rotate) * spawnData.CurrentPosition;
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