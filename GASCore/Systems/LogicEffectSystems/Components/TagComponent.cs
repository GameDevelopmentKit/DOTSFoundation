namespace GASCore.Systems.LogicEffectSystems.Components
{
    using GASCore.Interfaces;
    using Unity.Collections;
    using Unity.Entities;

    public struct TagComponent : IComponentData
    {
        public                          FixedString64Bytes Value;
        public static implicit operator FixedString64Bytes(TagComponent tag) => tag.Value;
        public static implicit operator TagComponent(FixedString64Bytes tag) => new() { Value = tag };

        public class _ : IAbilityActionComponentConverter
        {   
            public string Tag;

            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
            {
                ecb.AddComponent(index, entity, new TagComponent() { Value = this.Tag });
            }
        }
    }
}