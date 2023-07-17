#if USING_AGENT_NAVIGATION
namespace GASCore.Systems.TargetDetectionSystems.Systems
{
    using GASCore.Systems.VisualEffectSystems.Components;
    using ProjectDawn.Navigation;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Physics.Stateful;
    using Unity.Transforms;
    using static Unity.Entities.SystemAPI;
    using ShapeType = ProjectDawn.Navigation.ShapeType;

    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(FindTargetGroup))]
    public partial struct EntityColliderSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var spatial = GetSingletonRW<AgentSpatialPartitioningSystem.Singleton>();

            var job = new EntityColliderJob
            {
                Spatial = spatial.ValueRO,
            };
            job.ScheduleParallel();
        }
    }

    [BurstCompile]
    [WithAll(typeof(OnCollisionTag))]
    [WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)]
    partial struct EntityColliderJob : IJobEntity
    {
        [ReadOnly] public AgentSpatialPartitioningSystem.Singleton Spatial;

        public void Execute(Entity entity, in LocalToWorld transform, ref DynamicBuffer<StatefulTriggerEvent> statefulTriggerEvents, in EntityColliderData entityColliderData,
            EnabledRefRW<OnCollisionTag> onCollisionEnableState)
        {
            for (var index = 0; index < statefulTriggerEvents.Length;)
            {
                var triggerEvent = statefulTriggerEvents[index];
                if (triggerEvent.State == StatefulEventState.Exit)
                {
                    statefulTriggerEvents.RemoveAtSwapBack(index);
                }
                else
                {
                    triggerEvent.State           = StatefulEventState.Undefined;
                    statefulTriggerEvents[index] = triggerEvent;
                    index++;
                }
            }

            if (entityColliderData.ShapeType == VisualEffectSystems.Components.ShapeType.Sphere)
            {
                var action = new CirclesCollision
                {
                    Entity                = entity,
                    Transform             = transform,
                    EntityColliderData    = entityColliderData,
                    StatefulTriggerEvents = statefulTriggerEvents,
                };

                this.Spatial.QuerySphere(transform.Position, entityColliderData.Radius, ref action);
            }

            for (var index = 0; index < statefulTriggerEvents.Length; index++)
            {
                var triggerEvent = statefulTriggerEvents[index];
                if (triggerEvent.State == StatefulEventState.Undefined)
                {
                    triggerEvent.State           = StatefulEventState.Exit;
                    statefulTriggerEvents[index] = triggerEvent;
                }
            }

            onCollisionEnableState.ValueRW = statefulTriggerEvents.Length > 0;
        }

        struct CirclesCollision : ISpatialQueryEntity
        {
            public Entity                              Entity;
            public EntityColliderData                  EntityColliderData;
            public LocalToWorld                        Transform;
            public DynamicBuffer<StatefulTriggerEvent> StatefulTriggerEvents;

            public void Execute(Entity otherEntity, AgentBody otherBody, AgentShape otherShape, LocalTransform otherTransform)
            {
                float2 towards = this.Transform.Position.xz - otherTransform.Position.xz;

                float distancesq = math.lengthsq(towards);
                float radiusSum  = this.EntityColliderData.Radius + otherShape.Radius;
                if (distancesq > radiusSum * radiusSum || this.Entity == otherEntity)
                    return;

                var isStay = false;
                for (var index = 0; index < StatefulTriggerEvents.Length; index++)
                {
                    var triggerEvent = StatefulTriggerEvents[index];
                    if (triggerEvent.EntityB == otherEntity)
                    {
                        isStay                       = true;
                        triggerEvent.State           = StatefulEventState.Stay;
                        StatefulTriggerEvents[index] = triggerEvent;
                        break;
                    }
                }

                if (!isStay)
                {
                    StatefulTriggerEvents.Add(new StatefulTriggerEvent()
                    {
                        EntityA = this.Entity,
                        EntityB = otherEntity,
                        State   = StatefulEventState.Enter
                    });
                }
            }
        }

        struct CylindersCollision : ISpatialQueryEntity
        {
            public Entity         Entity;
            public AgentBody      Body;
            public AgentShape     Shape;
            public LocalTransform Transform;
            public float3         Displacement;
            public float          Weight;
            public float          ResolveFactor;

            public void Execute(Entity otherEntity, AgentBody otherBody, AgentShape otherShape, LocalTransform otherTransform)
            {
                if (otherShape.Type != ShapeType.Cylinder)
                    return;

                if (this.Body.IsStopped && !otherBody.IsStopped)
                    return;

                float extent      = this.Shape.Height * 0.5f;
                float otherExtent = otherShape.Height * 0.5f;
                if (math.abs((this.Transform.Position.y + extent) - (otherTransform.Position.y + otherExtent)) > extent + otherExtent)
                    return;

                float2 towards    = this.Transform.Position.xz - otherTransform.Position.xz;
                float  distancesq = math.lengthsq(towards);
                float  radius     = this.Shape.Radius + otherShape.Radius;
                if (distancesq > radius * radius || this.Entity == otherEntity)
                    return;

                float distance    = math.sqrt(distancesq);
                float penetration = radius - distance;

                if (distance < 0.0001f)
                {
                    // Avoid both having same displacement
                    if (otherEntity.Index > this.Entity.Index)
                    {
                        towards = -this.Body.Velocity.xz;
                    }
                    else
                    {
                        towards = this.Body.Velocity.xz;
                    }

                    if (math.length(towards) < 0.0001f)
                    {
                        float2 avoidDirection = new Random((uint)this.Entity.Index + 1).NextFloat2Direction();
                        towards = avoidDirection;
                    }

                    penetration = 0.01f;
                }
                else
                {
                    penetration = (penetration / distance) * this.ResolveFactor;
                }

                this.Displacement += new float3(towards.x, 0, towards.y) * penetration;
                this.Weight++;
            }
        }
    }
}
#endif