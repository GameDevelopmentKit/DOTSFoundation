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

    [UpdateInGroup(typeof(AbilityLogicEffectGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct SpawnEntitiesSystem : ISystem
    {
        private ComponentLookup<TeamOwnerId>    teamLookup;
        private ComponentLookup<WorldTransform> worldTransformLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            this.teamLookup           = state.GetComponentLookup<TeamOwnerId>(true);
            this.worldTransformLookup = state.GetComponentLookup<WorldTransform>(true);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            this.teamLookup.Update(ref state);
            this.worldTransformLookup.Update(ref state);
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            var random       = new Random((uint)(SystemAPI.Time.DeltaTime * 100000));

            new SpawnEntitiesJob()
            {
                Ecb                  = ecb,
                Random               = random,
                TeamLookup           = this.teamLookup,
                WorldTransformLookup = this.worldTransformLookup
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
        [ReadOnly] public ComponentLookup<WorldTransform>    WorldTransformLookup;

        private void Execute(
            Entity spawnerEntity,
            [EntityIndexInQuery] int index,
            ref EntitySpawner spawnData,
            in ActivatedStateEntityOwner activatedStateEntityOwner,
            in CasterComponent caster,
            in AbilityEffectId effectId,
            in AffectedTargetComponent affectedTarget,
            in WorldTransform spawnerTransform)
        {
            if (this.Random.NextFloat(0f, 1f) > spawnData.SpawnChance) return;

            if (spawnData.Clockwise == 0)
            {
                spawnData.CurrentAngle = this.Random.NextFloat(spawnData.StartAngleRange.min, spawnData.StartAngleRange.max);
                spawnData.Clockwise    = this.Random.NextBool() ? 1 : -1;
            }

            var amount        = this.Random.NextInt(spawnData.AmountRange.min, spawnData.AmountRange.max);
            var startEntitySpawnerRotation = spawnData.CurrentAngle;
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
                this.Ecb.SetComponent(index, newEntity, LocalTransform.FromPositionRotation(position, rotate));

                spawnData.CurrentAngle += this.Random.NextFloat(spawnData.AngleStepRange.min, spawnData.AngleStepRange.max) * spawnData.Clockwise;

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
            
        }
    }
}