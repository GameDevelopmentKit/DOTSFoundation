namespace AdventureWorld.CombatSystem.AbilitySystemExtension.Systems
{
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using GASCore.Systems.TargetDetectionSystems.Components;
    using GASCore.Systems.TargetDetectionSystems.Systems;
    using ProjectDawn.Navigation;
    using UnityEngine;
    using AdventureWorld.CombatSystem.AbilitySystemExtension.Components;

    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(FindTargetGroup))]
    public partial struct AimingNearestAgentInsideCastRangeSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            var spatial = SystemAPI.GetSingletonRW<AgentSpatialPartitioningSystem.Singleton>();
            new AimingNearestAgentJob
            {
                Spatial = spatial.ValueRO,
                UntargetableLookup = SystemAPI.GetComponentLookup<UntargetableTag>(true),
                CastRangeLookup = SystemAPI.GetComponentLookup<CastRangeComponent>(true),
                WorldTransformLookup = SystemAPI.GetComponentLookup<LocalToWorld>(true),
                AimingLookup = SystemAPI.GetComponentLookup<AimingComponent>(true),
                Ecb = ecb,
            }.ScheduleParallel();

        }
    }

    [BurstCompile]
    [WithAll(typeof(FindTargetTagComponent), typeof(AimNearestAgentInsideCastRange))]
    partial struct AimingNearestAgentJob : IJobEntity
    {
        [ReadOnly] public AgentSpatialPartitioningSystem.Singleton Spatial;
        [ReadOnly] public ComponentLookup<LocalToWorld> WorldTransformLookup;
        [ReadOnly] public ComponentLookup<CastRangeComponent> CastRangeLookup;
        [ReadOnly] public ComponentLookup<UntargetableTag> UntargetableLookup;
        [ReadOnly] public ComponentLookup<AimingComponent> AimingLookup;
        public EntityCommandBuffer.ParallelWriter Ecb;

        public void Execute([EntityIndexInQuery] int entityInQueryIndex, ref DynamicBuffer<TargetableElement> targetables, in CasterComponent caster, in ActivatedStateEntityOwner owner)
        {
            //multi thread, targetables must clear before use
            targetables.Clear(); 

            bool hasAimingComponent = AimingLookup.TryGetComponent(caster.Value, out AimingComponent aimingTarget);

            var casterPosition = this.WorldTransformLookup[caster].Position;
            var castRange = this.CastRangeLookup[owner];

            if (hasAimingComponent && aimingTarget.Value != Entity.Null && WorldTransformLookup.HasComponent(aimingTarget.Value))
            {
                //if aiming target is still in range, add it to targetables
                var targetPosition = this.WorldTransformLookup[aimingTarget.Value].Position;
                var distance = math.distancesq(casterPosition, targetPosition);

                if (distance <= castRange.ValueSqr)
                {
                    targetables.Add(aimingTarget.Value);
                    return;
                }
            }

            //create new aiming component for caster if not exist
            if (!hasAimingComponent) Ecb.AddComponent(entityInQueryIndex, caster.Value, new AimingComponent());

            //find nearest target if current aiming target is not in range, was destroyed or caster doesnt have an aiming component
            Entity nearestTarget = Entity.Null;
            var action = new NearestAgentQuery
            {
                UntargetableLookup = this.UntargetableLookup,
                CasterPos = casterPosition,
                CastRangeSq = castRange.ValueSqr,
                NearestDistanceSq = castRange.ValueSqr,
                NearestTarget = nearestTarget,
                Caster = caster.Value
            };

            this.Spatial.QuerySphere(casterPosition, castRange.Value, ref action);
            Ecb.SetComponent(entityInQueryIndex, caster.Value, new AimingComponent { Value = action.NearestTarget });
            targetables.Add(action.NearestTarget);
        }

        struct NearestAgentQuery : ISpatialQueryEntity
        {
            public ComponentLookup<UntargetableTag> UntargetableLookup;
            public float3 CasterPos;
            public float CastRangeSq;
            public float NearestDistanceSq;
            public Entity NearestTarget;
            public Entity Caster;

            public void Execute(Entity otherEntity, AgentBody otherBody, AgentShape otherShape, LocalToWorld otherTransform)
            {
                if (this.UntargetableLookup.HasComponent(otherEntity)) return;
                if (otherEntity.Equals(Caster)) return;
                var distanceSq = math.distancesq(this.CasterPos, otherTransform.Position);
                if (distanceSq > this.CastRangeSq) return;
                if (distanceSq < NearestDistanceSq)
                {
                    NearestDistanceSq = distanceSq;
                    NearestTarget = otherEntity;
                }
            }
        }
    }
}