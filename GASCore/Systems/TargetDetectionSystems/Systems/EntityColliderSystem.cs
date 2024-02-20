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
                var action = new SphereCollision
                {
                    Entity                = entity,
                    Transform             = transform,
                    EntityColliderData    = entityColliderData,
                    StatefulTriggerEvents = statefulTriggerEvents,
                };

                this.Spatial.QuerySphere(transform.Position + entityColliderData.Center, entityColliderData.Radius, ref action);
            }
            else if (entityColliderData.ShapeType == VisualEffectSystems.Components.ShapeType.Box)
            {
                var action = new BoxCollision
                {
                    Entity                = entity,
                    Transform             = transform,
                    EntityColliderData    = entityColliderData,
                    StatefulTriggerEvents = statefulTriggerEvents,
                };

                this.Spatial.QueryBox(transform.Position, entityColliderData.Center, transform.Rotation, entityColliderData.Size, ref action);
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

        struct SphereCollision : ISpatialQueryEntity
        {
            public Entity                              Entity;
            public EntityColliderData                  EntityColliderData;
            public LocalToWorld                        Transform;
            public DynamicBuffer<StatefulTriggerEvent> StatefulTriggerEvents;

            public void Execute(Entity otherEntity, AgentBody otherBody, AgentShape otherShape, LocalToWorld otherTransform)
            {
                var towards = this.Transform.Position - otherTransform.Position;

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

        struct BoxCollision : ISpatialQueryEntity
        {
            public Entity                              Entity;
            public EntityColliderData                  EntityColliderData;
            public LocalToWorld                        Transform;
            public DynamicBuffer<StatefulTriggerEvent> StatefulTriggerEvents;

            private bool CheckCollision(float3 centerA, float3 sizeA, quaternion rotationA, float3 centerB, float3 sizeB, quaternion rotationB)
            {
                // Calculate the half extents of the boxes
                float3 halfSizeA = math.mul(rotationA, sizeA * 0.5f);
                float3 halfSizeB = math.mul(rotationB, sizeB * 0.5f);

                // Calculate the minimum and maximum points defining the bounding boxes for each box
                float3 fromA = centerA - halfSizeA;
                float3 toA   = centerA + halfSizeA;
                float3 fromB = centerB - halfSizeB;
                float3 toB   = centerB + halfSizeB;

                var minA = math.min(fromA, toA);
                var maxA = math.max(fromA, toA);
                var minB = math.min(fromB, toB);
                var maxB = math.max(fromB, toB);

                // Check for overlap along each axis
                bool xOverlap = maxA.x >= minB.x && minA.x <= maxB.x;
                bool yOverlap = maxA.y >= minB.y && minA.y <= maxB.y;
                bool zOverlap = maxA.z >= minB.z && minA.z <= maxB.z;

                // If there is overlap along all axes, the boxes collide
                return xOverlap && yOverlap && zOverlap;
            }

            public void Execute(Entity otherEntity, AgentBody otherBody, AgentShape otherShape, LocalToWorld otherTransform)
            {
                if (this.Entity == otherEntity) return;
                var centerA = math.mul(this.Transform.Rotation, EntityColliderData.Center) + this.Transform.Position;
                var centerB = otherTransform.Position;
                var sizeB   = new float3(otherShape.Radius, otherShape.Height, otherShape.Radius);

                if (!this.CheckCollision(centerA, this.EntityColliderData.Size, this.Transform.Rotation, centerB, sizeB, otherTransform.Rotation)) return;
                var isStay = false;
                for (var index = 0; index < this.StatefulTriggerEvents.Length; index++)
                {
                    var triggerEvent = this.StatefulTriggerEvents[index];
                    if (triggerEvent.EntityB == otherEntity)
                    {
                        isStay                            = true;
                        triggerEvent.State                = StatefulEventState.Stay;
                        this.StatefulTriggerEvents[index] = triggerEvent;
                        break;
                    }
                }

                if (!isStay)
                {
                    this.StatefulTriggerEvents.Add(new StatefulTriggerEvent()
                    {
                        EntityA = this.Entity,
                        EntityB = otherEntity,
                        State   = StatefulEventState.Enter
                    });
                }
            }
        }
    }
}
#endif