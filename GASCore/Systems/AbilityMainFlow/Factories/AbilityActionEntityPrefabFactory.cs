namespace GASCore.Systems.AbilityMainFlow.Factories
{
    using System.Collections.Generic;
    using System.Linq;
    using DOTSCore.EntityFactory;
    using GASCore.Interfaces;
    using GASCore.Services;
    using GASCore.Systems.TimelineSystems.Components;
    using Unity.Collections;
    using Unity.Entities;

    /// <summary>
    /// Create ability action entity with some common components
    /// And it will be add more components depend on it is ability timeline entity or ability effect entity ...
    /// </summary>
    public class AbilityActionEntityPrefabFactory : BaseEntityPrefabFactoryByEcb<List<IComponentConverter>>
    {
        protected override void InitComponents(ref EntityCommandBuffer.ParallelWriter ecb, in int index, ref Entity actionEntity, in List<IComponentConverter> componentsData)
        {
            foreach (var component in componentsData)
            {
                component.Convert(ecb, index, actionEntity);
            }
        }

        public NativeList<Entity> CreateAbilityActionEntityPrefabsFromJson(EntityCommandBuffer.ParallelWriter ecb, int index, string jsonData, bool isTimelineEntity = false)
        {
            var listEntitiesData = jsonData.ConvertJsonToEntitiesData<IComponentConverter>();
            return this.CreateAbilityActionEntityPrefabsFromJson(ecb, index, listEntitiesData, isTimelineEntity);
        }

        public NativeList<Entity> CreateAbilityActionEntityPrefabsFromJson(EntityCommandBuffer.ParallelWriter ecb, int index, List<EntityConverter.EntityData<IComponentConverter>> listEntitiesData,
            bool isTimelineEntity = false)
        {
            var result = new NativeList<Entity>(listEntitiesData.Count, Allocator.Temp);

            foreach (var entityData in listEntitiesData)
            {
                var abilityActionEntity = this.CreateEntity(ecb, index, entityData.components);
                result.Add(abilityActionEntity);

                if (!isTimelineEntity) continue;
                //if component data contain any trigger condition, will be add TriggerConditionAmount
                var count = entityData.components.Count(converter => converter is ITriggerConditionActionConverter);
                if (count > 0)
                {
                    ecb.AddComponent(index, abilityActionEntity, new TriggerConditionAmount() { Value = count });
                    ecb.AddBuffer<CompletedTriggerElement>(index, abilityActionEntity);
                    ecb.AddComponent<InTriggerConditionResolveProcessTag>(index, abilityActionEntity);
                }
                else
                {
                    ecb.AddComponent<CompletedAllTriggerConditionTag>(index, abilityActionEntity);
                }
            }

            return result;
        }
    }
}