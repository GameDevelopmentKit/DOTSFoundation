namespace GASCore.Systems.CasterSystems.Systems
{
    using DOTSCore.Extension;
    using GameFoundation.Scripts.Utilities.Extension;
    using GASCore.Blueprints;
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Factories;
    using GASCore.Systems.CasterSystems.Components;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Scenes;
    using Zenject;

    [UpdateInGroup(typeof(GameAbilityInitializeSystemGroup), OrderFirst = true)]
    [RequireMatchingQueriesForUpdate]
    public partial class AbilityEntityPrefabPoolSystem : SystemBase
    {
        private AbilityBlueprint           abilityBlueprint;
        private AbilityEntityPrefabFactory abilityPrefabFactory;
        private Entity                     abilityPrefabHolderEntity;

        public struct Singleton : IComponentData
        {
            public NativeHashMap<FixedString64Bytes, Entity> AbilityNameToLevelPrefabs;
        }

        [Inject]
        public void Inject(AbilityBlueprint abilityBlueprintParam, AbilityEntityPrefabFactory abilityEntityPrefabFactory)
        {
            this.abilityBlueprint     = abilityBlueprintParam;
            this.abilityPrefabFactory = abilityEntityPrefabFactory;
        }

        protected override void OnCreate()
        {
            this.GetCurrentContainer().Inject(this);

            // create ability prefab holder
            this.abilityPrefabHolderEntity = this.EntityManager.CreateEntity();
            this.EntityManager.SetName(this.abilityPrefabHolderEntity, "AbilityPrefabHolder");


            this.EntityManager.AddComponentData(this.SystemHandle, new Singleton()
            {
                AbilityNameToLevelPrefabs = new NativeHashMap<FixedString64Bytes, Entity>(0, Allocator.Persistent),
            });
        }

        protected override void OnUpdate()
        {
            var abilityNameToLevelPrefabs = SystemAPI.GetSingletonRW<Singleton>().ValueRW.AbilityNameToLevelPrefabs;
            this.Entities.WithNone<PrefabLoadResult>().ForEach((Entity loaderEntity, in LoadAbilityPrefabData loadData) =>
            {
                FixedString64Bytes key = loadData.RequestData.AbilityLevelKey;
                if (!abilityNameToLevelPrefabs.ContainsKey(key))
                {
                    var abilityRecord      = this.abilityBlueprint.GetDataById(loadData.RequestData.AbilityId.Value);
                    var abilityLevelRecord = abilityRecord.LevelRecords[loadData.RequestData.Level - 1];
                    var abilityEntityPrefab = this.abilityPrefabFactory.CreateEntity(this.EntityManager, new AbilityFactoryModel()
                    {
                        AbilityRecord      = abilityRecord,
                        AbilityLevelRecord = abilityLevelRecord,
                        IsMaxLevel         = loadData.RequestData.Level == abilityRecord.LevelRecords.Count
                    });

                    abilityNameToLevelPrefabs.Add(key, abilityEntityPrefab);
                    this.EntityManager.SetParent(abilityEntityPrefab, this.abilityPrefabHolderEntity);
                }

                this.EntityManager.AddComponentData(loaderEntity, new PrefabLoadResult()
                {
                    PrefabRoot = abilityNameToLevelPrefabs[key]
                });
            }).WithoutBurst().WithStructuralChanges().Run();
        }
    }

    public struct LoadAbilityPrefabData : IComponentData
    {
        public RequestAddOrUpgradeAbility RequestData;
        public Entity                     CasterEntity;
    }
}