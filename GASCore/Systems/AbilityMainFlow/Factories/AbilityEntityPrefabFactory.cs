namespace GASCore.Systems.AbilityMainFlow.Factories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using DOTSCore.EntityFactory;
    using DOTSCore.Extension;
    using GASCore.Blueprints;
    using GASCore.Interfaces;
    using GASCore.Services;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using GASCore.Systems.LogicEffectSystems.Factories;
    using GASCore.Systems.TimelineSystems.Components;
    using GASCore.Systems.TimelineSystems.Factories;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Transforms;

    public struct AbilityFactoryModel
    {
        public AbilityRecord      AbilityRecord;
        public AbilityLevelRecord AbilityLevelRecord;
        public bool               IsMaxLevel;
    }

    public class AbilityEntityPrefabFactory : BaseEntityPrefabFactory<AbilityFactoryModel>
    {
        #region Inject

        private readonly AbilityEffectEntityPrefabFactory   abilityEffectEntityPrefabFactory;
        private readonly AbilityTimelineEntityPrefabFactory timelineEntityPrefabFactory;

        #endregion

        public AbilityEntityPrefabFactory(AbilityEffectEntityPrefabFactory abilityEffectEntityPrefabFactory, AbilityTimelineEntityPrefabFactory timelineEntityPrefabFactory) : base()
        {
            this.abilityEffectEntityPrefabFactory = abilityEffectEntityPrefabFactory;
            this.timelineEntityPrefabFactory      = timelineEntityPrefabFactory;
        }

        protected override void InitComponents(EntityManager entityManager, Entity abilityEntity, AbilityFactoryModel abilityFactoryModel)
        {
            var ecb         = new EntityCommandBuffer(Allocator.TempJob);
            var ecbParallel = ecb.AsParallelWriter();
            var index       = 0;

            var record = abilityFactoryModel.AbilityRecord;
            // ====== Ability general info ======
            ecbParallel.SetName(index, abilityEntity, $"{record.Id}_Lv{abilityFactoryModel.AbilityLevelRecord.LevelIndex + 1}");
            ecbParallel.AddComponent(index, abilityEntity, new ComponentTypeSet(typeof(LocalToWorld)));
            ecbParallel.AddComponent(index, abilityEntity, new ComponentTypeSet(typeof(RequestActivate), typeof(GrantedActivation), typeof(ActivatedTag)));
            ecbParallel.AddComponent(index, abilityEntity, new AbilityId() { Value    = record.Id });
            ecbParallel.AddComponent(index, abilityEntity, new AbilityLevel() { Value = abilityFactoryModel.AbilityLevelRecord.LevelIndex + 1 });
            ecbParallel.AddComponent(index, abilityEntity, new Duration() { Value     = 0 });
            ecbParallel.SetComponentEnabled<RequestActivate>(index, abilityEntity, false);
            ecbParallel.SetComponentEnabled<GrantedActivation>(index, abilityEntity, false);
            ecbParallel.SetComponentEnabled<ActivatedTag>(index, abilityEntity, false);

            if (abilityFactoryModel.IsMaxLevel)
                ecbParallel.AddComponent<MaxLevelTag>(index, abilityEntity);

            //attach ability type tag
            if (record.Type == AbilityType.Active)
                ecbParallel.AddComponent(index, abilityEntity, new ActiveAbilityTag());
            else if (record.Type == AbilityType.Passive)
                ecbParallel.AddComponent(index, abilityEntity, new PassiveAbilityTag());

            // ===== Ability Level Info ======
            var levelRecord = abilityFactoryModel.AbilityLevelRecord;
            ecbParallel.AddComponent(index, abilityEntity, new CastRangeComponent() { Value = levelRecord.CastRange });
            ecbParallel.AddComponent(index, abilityEntity, new Cooldown() { Value           = levelRecord.Cooldown });

            // add ability cost buffer
            var abilityCostBuffer = ecbParallel.AddBuffer<AbilityCost>(index, abilityEntity);
            foreach (var (name, value) in levelRecord.Cost)
            {
                abilityCostBuffer.Add(new AbilityCost() { Name = name, Value = value });
            }

            //setup to auto activate ability by trigger condition 
            var triggerComponentsData = levelRecord.AbilityActivateCondition.ConvertJsonToComponentsData<IComponentConverter>();
            if (triggerComponentsData?.Count > 0)
            {
                ecbParallel.AddComponent<AutoActiveTag>(index, abilityEntity);
                var count = 0;
                foreach (var component in triggerComponentsData)
                {
                    component.Convert(ecbParallel, index, abilityEntity);
                    if (component is not RecycleTriggerEntityTag._) count++;
                }

                //if component data contain any trigger condition, will be add TriggerConditionCount
                ecbParallel.SetupTriggerCondition(index, abilityEntity, count);
            }

            // setup ability timeline prefab
            var timelineEntity = this.timelineEntityPrefabFactory.CreateEntity(ecbParallel, index, levelRecord.AbilityTimeline);
            ecbParallel.SetParent(index, timelineEntity, abilityEntity);
            ecbParallel.AddComponent(index, abilityEntity, new AbilityTimelinePrefabComponent() { Value = timelineEntity });

            // setup ability effect pool
            var effectPoolBuffer = ecbParallel.AddBuffer<AbilityEffectElement>(index, abilityEntity);
            foreach (var effect in levelRecord.AbilityEffectPool)
            {
                var effectPrefab = this.abilityEffectEntityPrefabFactory.CreateEntity(ecbParallel, index, effect);
                effectPoolBuffer.Add(new AbilityEffectElement() { EffectPrefab = effectPrefab });
                ecbParallel.SetParent(index, effectPrefab, abilityEntity);
            }

            var cacheLinkEntityBuffer = ecbParallel.AddBuffer<LinkedEntityGroup>(index, abilityEntity);
            cacheLinkEntityBuffer.Add(new LinkedEntityGroup() { Value = abilityEntity });

            ecb.Playback(entityManager);
            ecb.Dispose();
        }
    }
}