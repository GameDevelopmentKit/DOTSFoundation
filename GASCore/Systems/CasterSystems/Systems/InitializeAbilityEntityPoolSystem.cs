namespace GASCore.Systems.CasterSystems.Systems
{
    using DOTSCore.Extension;
    using GameFoundation.Scripts.Utilities.Extension;
    using GASCore.Blueprints;
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.AbilityMainFlow.Factories;
    using GASCore.Systems.CasterSystems.Components;
    using Unity.Collections;
    using Unity.Entities;
    using Zenject;

    [UpdateInGroup(typeof(GameAbilityInitializeSystemGroup), OrderFirst = true)]
    [RequireMatchingQueriesForUpdate]
    public partial class InitializeAbilityEntityPoolSystem : SystemBase
    {
        private AbilityBlueprint           abilityBlueprint;
        private AbilityEntityPrefabFactory abilityPrefabFactory;


        [Inject]
        public void Inject(AbilityBlueprint abilityBlueprintParam, AbilityEntityPrefabFactory abilityEntityPrefabFactory)
        {
            this.abilityBlueprint     = abilityBlueprintParam;
            this.abilityPrefabFactory = abilityEntityPrefabFactory;
        }

        protected override void OnCreate()
        {
            this.RequireForUpdate<AbilityPrefabPool>();

            this.GetCurrentContainer().Inject(this);
            var abilityNameToLevelPrefabs = new NativeParallelHashMap<FixedString64Bytes, Entity>(this.abilityBlueprint.Count, Allocator.Persistent);
            var abilityPrefabHolderEntity = EntityManager.CreateEntity();
            EntityManager.SetName(abilityPrefabHolderEntity, "AbilityPrefabHolder");

            foreach (var abilityRecord in this.abilityBlueprint.Values)
            {
                foreach (var abilityLevelRecord in abilityRecord.LevelRecords)
                {
                    var abilityEntityPrefab = this.abilityPrefabFactory.CreateEntity(EntityManager, new AbilityFactoryModel()
                    {
                        AbilityRecord      = abilityRecord,
                        AbilityLevelRecord = abilityLevelRecord
                    });
                    var levelIndex = abilityLevelRecord.LevelIndex + 1;
                    abilityNameToLevelPrefabs.Add($"{abilityRecord.Id}_{levelIndex}", abilityEntityPrefab);

                    if (levelIndex == abilityRecord.LevelRecords.Count)
                    {
                        EntityManager.AddComponent<MaxLevelTag>(abilityEntityPrefab);
                    }

                    EntityManager.SetParent(abilityEntityPrefab, abilityPrefabHolderEntity);
                }
            }

            EntityManager.AddComponentData(abilityPrefabHolderEntity, new AbilityPrefabPool() { AbilityNameToLevelPrefabs = abilityNameToLevelPrefabs.CreateReference() });
        }

        protected override void OnUpdate() { }
    }
}