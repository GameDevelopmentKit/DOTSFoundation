namespace GASCore.Systems.VisualEffectSystems.Systems
{
    using System;
    using DOTSCore.Extension;
    using GASCore.Groups;
    using GASCore.Interfaces;
    using GASCore.Systems.LogicEffectSystems.Components;
    using Newtonsoft.Json;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;
    using UnityEngine;

    [Serializable]
    public struct JumpToTargetData : IComponentData, IAbilityActionComponentConverter
    {
        public float TimeToTarget;

        public const                          float  Gravity = 10.5f;
        [JsonIgnore] [HideInInspector] public bool   IsInitialized;
        [JsonIgnore] [HideInInspector] public float3 InitialVelocityVector;

        public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity) { ecb.AddComponent(index, entity, this); }

        public float3 GetVelocityVector(float3 target, float3 origin)
        {
            var direction   = target - origin;
            var directionXZ = new float3(direction.x, 0.0f, direction.z);

            var distanceXZ = math.length(directionXZ);
            var distanceY  = direction.y;

            var velocityY  = distanceY / this.TimeToTarget + 0.5f * Gravity * this.TimeToTarget;
            var velocityXZ = distanceXZ / this.TimeToTarget;

            var velocity = math.normalize(directionXZ) * velocityXZ;
            velocity.y = velocityY;

            return velocity;
        }
    }

    [UpdateInGroup(typeof(AbilityVisualEffectGroup), OrderLast = true)]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct JumpToTargetSystem : ISystem
    {
        private float3 upVector;
        public  void   OnCreate(ref SystemState state) { this.upVector = math.up(); }
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new JumpToTargetJob()
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
                UpVector  = this.upVector
            }.ScheduleParallel();
        }
    }

    [BurstCompile]
    [WithNone(typeof(MovementDirection))]
    public partial struct JumpToTargetJob : IJobEntity
    {
        public float  DeltaTime;
        public float3 UpVector;
        void Execute(ref LocalTransform transform, in TargetPosition targetPosition, ref JumpToTargetData jumpToTargetData)
        {
            if (math.distancesq(transform.Position,targetPosition.Value).IsZero(0.1f)) return;
            if (!jumpToTargetData.IsInitialized)
            {
                jumpToTargetData.IsInitialized         = true;
                jumpToTargetData.InitialVelocityVector = jumpToTargetData.GetVelocityVector(targetPosition.Value, transform.Position);
            }

            transform.Position                     += jumpToTargetData.InitialVelocityVector * DeltaTime - 0.5f * JumpToTargetData.Gravity * DeltaTime * DeltaTime * this.UpVector;
            jumpToTargetData.InitialVelocityVector -= JumpToTargetData.Gravity * DeltaTime * this.UpVector;
        }
    }
}