namespace GASCore.Systems.VisualEffectSystems.Components
{
    using GASCore.Interfaces;
    using GASCore.Systems.AbilityMainFlow.Components;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Physics.Stateful;

#if UNITY_PHYSICS_CUSTOM
    public class PhysicsCategoryTagsConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                writer.WriteValue(((PhysicsCategoryTags)value).Value);
            }
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return new PhysicsCategoryTags() { Value = uint.TryParse(reader.Value?.ToString(), out var tagsValue) ? tagsValue : 0 };
        }
    
        public override bool CanConvert(Type objectType) { return objectType == typeof(PhysicsCategoryTags); }
    }
#endif

    public struct EntityColliderData : IComponentData
    {
        public ShapeType ShapeType;
        public float3    Size;
        public float     Radius;
    }

    public struct OnCollisionTag : IComponentData, IEnableableComponent
    {
        
    }

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
        
        [JsonConverter(typeof(PhysicsCategoryTagsConverter))]
        public PhysicsCategoryTags ColliderBelongsTo;
        
        [JsonConverter(typeof(PhysicsCategoryTagsConverter))]
        public PhysicsCategoryTags ColliderCollidesWith;
#endif

        public bool IsDestroyOnHit;
        
        public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
        {
#if UNITY_PHYSICS_CUSTOM
            ecb.AddBoxPhysicsCollider(index, entity, this.Size, this.Center, this.CollisionResponse, this.ColliderBelongsTo, this.ColliderCollidesWith);
#else
            ecb.AddComponent(index, entity, new EntityColliderData() { Size = this.Size, Radius = this.Size.x, ShapeType = ShapeType.Sphere });
            ecb.AddComponent<OnCollisionTag>(index, entity);
            ecb.SetComponentEnabled<OnCollisionTag>(index, entity,false);
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
        
        [JsonConverter(typeof(PhysicsCategoryTagsConverter))]
        public PhysicsCategoryTags ColliderBelongsTo;
        
        [JsonConverter(typeof(PhysicsCategoryTagsConverter))]
        public PhysicsCategoryTags ColliderCollidesWith;
#endif

        public bool IsDestroyOnHit;
        
        public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
        {
#if UNITY_PHYSICS_CUSTOM
            ecb.AddSpherePhysicsCollider(index, entity, this.Radius, this.Center, this.CollisionResponse, this.ColliderBelongsTo, this.ColliderCollidesWith);
#else
            ecb.AddComponent(index, entity, new EntityColliderData() { Radius = this.Radius, ShapeType = ShapeType.Sphere });
            ecb.AddComponent<OnCollisionTag>(index, entity);
            ecb.SetComponentEnabled<OnCollisionTag>(index, entity,false);
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