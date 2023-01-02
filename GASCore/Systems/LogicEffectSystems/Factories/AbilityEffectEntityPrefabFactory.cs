namespace GASCore.Systems.LogicEffectSystems.Factories
{
    using DOTSCore.EntityFactory;
    using DOTSCore.Extension;
    using GASCore.Blueprints;
    using GASCore.Systems.AbilityMainFlow.Factories;
    using GASCore.Systems.LogicEffectSystems.Components;
    using Unity.Entities;
    using Unity.Transforms;

    public class AbilityEffectEntityPrefabFactory : BaseEntityPrefabFactoryByEcb<AbilityEffect>
    {
        private readonly AbilityActionEntityPrefabFactory actionEntityPrefabFactory;
        public AbilityEffectEntityPrefabFactory(AbilityActionEntityPrefabFactory actionEntityPrefabFactory) : base() { this.actionEntityPrefabFactory = actionEntityPrefabFactory; }
        protected override void InitComponents(ref EntityCommandBuffer.ParallelWriter ecb, in int index, ref Entity effectEntity, in AbilityEffect data)
        {
            ecb.SetName(index, effectEntity, data.EffectId + "Prefab");
            ecb.AddComponent(index, effectEntity, new AbilityEffectId() { Value = data.EffectId });

            // add target type buffer
            var targetTypeBuffer = ecb.AddBuffer<AffectedTargetTypeElement>(index, effectEntity);
            foreach (var target in data.AffectedTarget)
            {
                targetTypeBuffer.Add(new AffectedTargetTypeElement() { Value = target });
            }

            // parse json data to effect action entity prefab
            var listEntityActionPrefab = this.actionEntityPrefabFactory.CreateAbilityActionEntityPrefabsFromJson(ecb, index, data.EffectData);
            ecb.AddChildren(index, effectEntity, listEntityActionPrefab);
        }
    }
}