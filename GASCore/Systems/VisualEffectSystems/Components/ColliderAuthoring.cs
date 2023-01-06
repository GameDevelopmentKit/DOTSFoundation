namespace GASCore.Systems.VisualEffectSystems.Components
{
    using System;
    using DOTSCore.UnityPhysicExtension.Utils;
    using GASCore.Interfaces;
    using Newtonsoft.Json;
    using Unity.Entities;
    using Unity.Physics;
    using Unity.Physics.Authoring;
    using Unity.Physics.Stateful;

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

    public class BoxColliderAuthoring : IAbilityActionComponentConverter
    {
        public SimpleVector3           Size              = new() { x = 1, y = 1, z = 1 };
        public SimpleVector3           Center            = new() { x = 0, y = 0, z = 0 };
        public CollisionResponsePolicy CollisionResponse = CollisionResponsePolicy.RaiseTriggerEvents;

        [JsonConverter(typeof(PhysicsCategoryTagsConverter))]
        public PhysicsCategoryTags ColliderBelongsTo;

        [JsonConverter(typeof(PhysicsCategoryTagsConverter))]
        public PhysicsCategoryTags ColliderCollidesWith;

        public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
        {
            ecb.AddBoxPhysicsCollider(index, entity, this.Size, this.Center, this.CollisionResponse, this.ColliderBelongsTo, this.ColliderCollidesWith);
        }
    }

    public class SphereColliderAuthoring : IAbilityActionComponentConverter
    {
        public float                   Radius            = 1;
        public SimpleVector3           Center            = new() { x = 0, y = 0, z = 0 };
        public CollisionResponsePolicy CollisionResponse = CollisionResponsePolicy.RaiseTriggerEvents;

        [JsonConverter(typeof(PhysicsCategoryTagsConverter))]
        public PhysicsCategoryTags ColliderBelongsTo;

        [JsonConverter(typeof(PhysicsCategoryTagsConverter))]
        public PhysicsCategoryTags ColliderCollidesWith;

        public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
        {
            ecb.AddSpherePhysicsCollider(index, entity, this.Radius, this.Center, this.CollisionResponse, this.ColliderBelongsTo, this.ColliderCollidesWith);
        }
    }

    public class StatefulTriggerEventAuthoring : IAbilityActionComponentConverter
    {
        public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity) { ecb.AddBuffer<StatefulTriggerEvent>(index, entity); }
    }
}