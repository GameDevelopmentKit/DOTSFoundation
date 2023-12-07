namespace GASCore.Systems.TargetDetectionSystems.Components
{
    using System.Collections.Generic;
    using GASCore.Interfaces;
    using Unity.Entities;
    using UnityEngine;

    public struct FindTargetComponent : IComponentData, IEnableableComponent
    {
        public bool WaitForOtherTriggers;
    }

    public struct OverrideFindTargetTag : IComponentData { }

    [InternalBufferCapacity(0)]
    public struct TargetableElement : IBufferElementData
    {
        public                          Entity Value;
        public static implicit operator Entity(TargetableElement target) => target.Value;
        public static implicit operator TargetableElement(Entity entity) => new() { Value = entity };
    }

    public class FindTargetAuthoring : ITriggerConditionActionConverter
    {
        public interface IOptionConverter : IComponentConverter { }

        public                      bool                   WaitForOtherTriggers = true;
        [SerializeReference] public List<IOptionConverter> Options              = new();

        public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
        {
            ecb.AddComponent(index, entity, new FindTargetComponent
            {
                WaitForOtherTriggers = this.WaitForOtherTriggers,
            });
            ecb.SetComponentEnabled<FindTargetComponent>(index, entity, false);
            foreach (var option in this.Options)
            {
                option.Convert(ecb, index, entity);
            }
        }
    }
}