namespace GASCore.Systems.VisualEffectSystems.Components
{
    using GASCore.Interfaces;
    using GASCore.Systems.AbilityMainFlow.Components;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Physics.Stateful;

#if UNITY_PHYSICS_CUSTOM
    using DOTSCore.UnityPhysicExtension.Utils;
    using Unity.Physics;
    using UnityEngine;
#endif

    public struct EntityColliderData : IComponentData
    {
        public ShapeType ShapeType;
        public float3    Size;
        public float3    Center;
        public float     Radius;
    }

    public struct OnCollisionTag : IComponentData, IEnableableComponent { }

    public enum ShapeType
    {
        Box,
        Sphere
    }

    public class BoxColliderAuthoring : IAbilityActionComponentConverter
    {
        public SimpleVector3 Size = new() { x = 1, y = 1, z = 1 };

        public SimpleVector3 Center = new() { x = 0, y = 0, z = 0 };

#if UNITY_PHYSICS_CUSTOM
        public CollisionResponsePolicy CollisionResponse = CollisionResponsePolicy.RaiseTriggerEvents;
        
        public LayerMask ColliderBelongsTo;
        
        public LayerMask ColliderCollidesWith;
#endif

        public bool IsDestroyOnHit;

        public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
        {
#if UNITY_PHYSICS_CUSTOM
            ecb.AddBoxPhysicsCollider(index, entity, this.Size, this.Center, this.CollisionResponse, this.ColliderBelongsTo.value, this.ColliderCollidesWith.value);
#else
            ecb.AddComponent(index, entity, new EntityColliderData() { Size = this.Size, ShapeType = ShapeType.Box, Center = this.Center});
            ecb.AddComponent<OnCollisionTag>(index, entity);
            ecb.SetComponentEnabled<OnCollisionTag>(index, entity, false);
#endif
            ecb.AddBuffer<StatefulTriggerEvent>(index, entity);

            if (this.IsDestroyOnHit)
            {
                ecb.AddComponent<ForceCleanupTag>(index, entity);
                ecb.SetComponentEnabled<ForceCleanupTag>(index, entity, false);
            }
        }
    }

    public class SphereColliderAuthoring : IAbilityActionComponentConverter
    {
        public float Radius = 1;

        public SimpleVector3 Center = new() { x = 0, y = 0, z = 0 };

#if UNITY_PHYSICS_CUSTOM
        public CollisionResponsePolicy CollisionResponse = CollisionResponsePolicy.RaiseTriggerEvents;
        
        public LayerMask ColliderBelongsTo;
        
        public LayerMask ColliderCollidesWith;
#endif

        public bool IsDestroyOnHit;

        public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
        {
#if UNITY_PHYSICS_CUSTOM
            ecb.AddSpherePhysicsCollider(index, entity, this.Radius, this.Center, this.CollisionResponse, this.ColliderBelongsTo.value, this.ColliderCollidesWith.value);
#else
            ecb.AddComponent(index, entity, new EntityColliderData() { Radius = this.Radius, ShapeType = ShapeType.Sphere, Center = this.Center});
            ecb.AddComponent<OnCollisionTag>(index, entity);
            ecb.SetComponentEnabled<OnCollisionTag>(index, entity, false);
#endif
            ecb.AddBuffer<StatefulTriggerEvent>(index, entity);

            if (this.IsDestroyOnHit)
            {
                ecb.AddComponent<ForceCleanupTag>(index, entity);
                ecb.SetComponentEnabled<ForceCleanupTag>(index, entity, false);
            }
        }
    }
}