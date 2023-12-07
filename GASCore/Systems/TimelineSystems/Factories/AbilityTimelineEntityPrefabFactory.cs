namespace GASCore.Systems.TimelineSystems.Factories
{
    using DOTSCore.EntityFactory;
    using DOTSCore.Extension;
    using GASCore.Systems.AbilityMainFlow.Factories;
    using GASCore.Systems.TimelineSystems.Components;
    using Unity.Entities;

    public class AbilityTimelineEntityPrefabFactory : BaseEntityPrefabFactoryByEcb<string>
    {
        private readonly AbilityActionEntityPrefabFactory actionEntityPrefabFactory;
        public AbilityTimelineEntityPrefabFactory(AbilityActionEntityPrefabFactory actionEntityPrefabFactory) : base() { this.actionEntityPrefabFactory = actionEntityPrefabFactory; }

        protected override void InitComponents(ref EntityCommandBuffer.ParallelWriter ecb, in int index, ref Entity timelineEntity, in string data)
        {
            ecb.SetName(index, timelineEntity, "TimelineEntityPrefab");

            // parse json data to timeline action entity prefab
            var listEntityActionPrefab = this.actionEntityPrefabFactory.CreateAbilityActionEntityPrefabsFromJson(ecb, index, data, true);
            ecb.AddChildren(index, timelineEntity, listEntityActionPrefab);
            ecb.AddComponent(index, timelineEntity, new AbilityTimelineEntitiesAmountComponent { Value = listEntityActionPrefab.Length });
        }
    }
}