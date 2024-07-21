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
                //this.Spatial.QueryAlongOrientedBox(transform.Position + math.mul(transform.Rotation, entityColliderData.Center), entityColliderData.Size, transform.Rotation, ref action);
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

        struct BoxCollision : ISpatialQueryEntity
        {
            public Entity                              Entity;
            public EntityColliderData                  EntityColliderData;
            public LocalToWorld                        Transform;
            public DynamicBuffer<StatefulTriggerEvent> StatefulTriggerEvents;


            public bool CheckCollision(BoxInfo box1, BoxInfo box2)
            {
                // Convert both boxes into their local space
                float4x4 box1ToLocal = math.inverse(float4x4.TRS(box1.Center, box1.WorldTransform.Rotation, new float3(1)));

                // Convert the second box into the first box's local space
                float4x4 box2ToBox1 = math.mul(box1ToLocal, box2.WorldTransform.Value);

                // Iterate through all axes for separation
                using var axes = box1.GetAxes();
                foreach (float3 axis in axes)
                {
                    if (!OverlapOnAxis(axis, box1, box2, box2ToBox1))
                    {
                        // If there is no overlap on any axis, the boxes are not colliding
                        return false;
                    }
                }

                // If overlap occurs on all axes, the boxes are colliding
                return true;
            }

            private bool OverlapOnAxis(float3 axis, BoxInfo box1, BoxInfo box2, float4x4 box2ToBox1)
            {
                // Project vertices of both boxes onto the axis
                float thisMin  = float.MaxValue, thisMax  = float.MinValue;
                float otherMin = float.MaxValue, otherMax = float.MinValue;

                var thisVertices  = box1.GetVertices();
                var otherVertices = box2.GetVertices();

                foreach (float3 vertex in thisVertices)
                {
                    float projection = math.dot(vertex, axis);
                    thisMin = math.min(thisMin, projection);
                    thisMax = math.max(thisMax, projection);
                }

                thisVertices.Dispose();

                foreach (float3 vertex in otherVertices)
                {
                    float projection = math.dot(vertex, axis);
                    otherMin = math.min(otherMin, projection);
                    otherMax = math.max(otherMax, projection);
                }

                otherVertices.Dispose();

                // Apply transformation between boxes
                float3 thisMinInOtherSpace = math.mul(box2ToBox1, new float4(thisMin * axis, 1.0f)).xyz;
                float3 thisMaxInOtherSpace = math.mul(box2ToBox1, new float4(thisMax * axis, 1.0f)).xyz;

                // Check for overlap
                return thisMaxInOtherSpace.x >= otherMin && otherMax >= thisMinInOtherSpace.x &&
                       thisMaxInOtherSpace.y >= otherMin && otherMax >= thisMinInOtherSpace.y &&
                       thisMaxInOtherSpace.z >= otherMin && otherMax >= thisMinInOtherSpace.z;
            }


            public struct BoxInfo
            {
                public float3       Center;
                public float3       Size;
                public LocalToWorld WorldTransform;

                public NativeArray<float3> GetVertices()
                {
                    var corners  = new NativeArray<float3>(8, Allocator.Temp);
                    var halfSize = math.mul(this.WorldTransform.Rotation, this.Size * 0.5f);

                    corners[0] = this.Center + halfSize;
                    corners[1] = this.Center + new float3(halfSize.x, -halfSize.y, halfSize.z);
                    corners[2] = this.Center + new float3(-halfSize.x, halfSize.y, halfSize.z);
                    corners[3] = this.Center + new float3(-halfSize.x, -halfSize.y, halfSize.z);
                    corners[4] = this.Center + new float3(halfSize.x, halfSize.y, -halfSize.z);
                    corners[5] = this.Center + new float3(halfSize.x, -halfSize.y, -halfSize.z);
                    corners[6] = this.Center + new float3(-halfSize.x, halfSize.y, -halfSize.z);
                    corners[7] = this.Center - halfSize;

                    return corners;
                }

                public NativeArray<float3> GetAxes()
                {
                    // Get the local space axes of the oriented box
                    var axes = new NativeArray<float3>(3, Allocator.Temp);
                    axes[0] = this.WorldTransform.Right;
                    axes[1] = this.WorldTransform.Up;
                    axes[2] = this.WorldTransform.Forward;
                    return axes;
                }
            }


            public void Execute(Entity otherEntity, AgentBody otherBody, AgentShape otherShape, LocalToWorld otherTransform)
            {
                if (this.Entity == otherEntity) return;
                // if (!this.CheckCollision(new BoxInfo()
                //     {
                //         Center         = math.mul(this.Transform.Rotation, EntityColliderData.Center) + this.Transform.Position,
                //         Size           = this.EntityColliderData.Size,
                //         WorldTransform = this.Transform
                //     }, new BoxInfo()
                //     {
                //         Center         = otherTransform.Position,
                //         Size           = new float3(otherShape.Radius, otherShape.Height, otherShape.Radius),
                //         WorldTransform = otherTransform
                //     })) return;

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