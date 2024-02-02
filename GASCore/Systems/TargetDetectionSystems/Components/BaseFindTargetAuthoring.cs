namespace GASCore.Systems.TargetDetectionSystems.Components
{
    using System.Collections.Generic;
    using GASCore.Interfaces;
    using Unity.Entities;
    using UnityEngine;

    public class BaseFindTargetAuthoring : ITriggerConditionActionConverter
    {
        public bool WaitForOtherTriggers = true;

        [SerializeReference] public List<IFilterTargetConverter> Options = new();

        public virtual void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
        {
            ecb.AddComponent(index, entity, new FindTargetTagComponent
            {
                WaitForOtherTriggers = this.WaitForOtherTriggers,
            });
            ecb.SetComponentEnabled<FindTargetTagComponent>(index, entity, false);
            foreach (var option in this.Options)
            {
                option.Convert(ecb, index, entity);
            }
        }
    }

    public interface IFilterTargetConverter : IComponentConverter { }
}