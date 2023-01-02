namespace DOTSCore.PrefabWorkflow
{
    using System;
    using System.Collections.Generic;
    using GameFoundation.Scripts.Utilities.Extension;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Entities.Serialization;
    using Unity.Mathematics;
    using Unity.Scenes;
    using Unity.Transforms;
    using Zenject;

    [DisableAutoCreation]
    [RequireMatchingQueriesForUpdate]
    public partial class PrefabEntitySpawnerSystem : SystemBase
    {
        private EntityPrefabGuidResolver                 prefabDatabase;
        private BeginSimulationEntityCommandBufferSystem beginSimEcbSystem;

        private readonly Dictionary<string, Entity>               prefabNameToEntity       = new();
        private readonly Dictionary<string, List<Action<Entity>>> prefabNameToListCallback = new();

        [Inject]
        public void Inject(PrefabDatabase prefabDatabaseParam) { this.prefabDatabase = prefabDatabaseParam.EntityPrefabGuidResolver; }

        protected override void OnCreate()
        {
            this.GetCurrentContainer().Inject(this);
            this.beginSimEcbSystem = this.World.GetExistingSystemManaged<BeginSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var ecb  = this.beginSimEcbSystem.CreateCommandBuffer().AsParallelWriter();
            var temp = this.prefabDatabase;
            //Add request to load a prefab
            this.Entities.WithNone<RequestEntityPrefabLoaded>().WithReadOnly(temp).ForEach((Entity entity, int entityInQueryIndex, in PrefabEntitySpawnerComponent spawner) =>
            {
                ecb.AddComponent(entityInQueryIndex, entity,
                    new RequestEntityPrefabLoaded { Prefab = new EntityPrefabReference(temp.GetPrefabEntityGuid(spawner.PrefabName)) });
            }).ScheduleParallel();


            var temp2 = this.GetComponentLookup<SetParentComponent>(true);
            //when prefab was loaded, spawn instance from prefab
            this.Entities
                .WithBurst(FloatMode.Default, FloatPrecision.Standard, true).WithReadOnly(temp2)
                .WithNone<PrefabSpawnedSignal>().ForEach((Entity entity, int entityInQueryIndex, in PrefabEntitySpawnerComponent spawner, in PrefabLoadResult prefab) =>
                {
                    var instance = ecb.Instantiate(entityInQueryIndex, prefab.PrefabRoot);
                    ecb.SetComponent(entityInQueryIndex, instance, new Translation() { Value = spawner.Position });
                    ecb.SetName(entityInQueryIndex, instance, spawner.PrefabName);
                    if (temp2.TryGetComponent(entity, out var componentData))
                    {
                        ecb.AddComponent(entityInQueryIndex, instance, new Parent() { Value = componentData.ParentEntity });
                        ecb.AddComponent(entityInQueryIndex, instance, new LocalToParent());
                    }

                    ecb.AddComponent(entityInQueryIndex, entity, new PrefabSpawnedSignal() { PrefabEntity = instance, PrefabName = spawner.PrefabName });
                }).ScheduleParallel();

            this.beginSimEcbSystem.AddJobHandleForProducer(this.Dependency);

            this.Entities.WithoutBurst().WithStructuralChanges().ForEach((Entity entity, int entityInQueryIndex, in PrefabSpawnedSignal prefabSpawnedSignal) =>
            {
                var prefabNameValue = prefabSpawnedSignal.PrefabName.Value;
                this.prefabNameToEntity.Add(prefabNameValue, prefabSpawnedSignal.PrefabEntity);
                if (!this.prefabNameToListCallback.TryGetValue(prefabNameValue, out var list)) return;
                foreach (var action in list)
                {
                    action.Invoke(prefabSpawnedSignal.PrefabEntity);
                }

                this.EntityManager.DestroyEntity(entity);
            }).Run();
        }

        public void LoadPrefabEntity(string name, Action<Entity> callback = null, float3 position = default, Entity parent = default)
        {
            if (this.prefabNameToEntity.TryGetValue(name, out var result))
            {
                callback?.Invoke(result);
            }
            else
            {
                if (!this.prefabNameToListCallback.TryGetValue(name, out var listCallback))
                {
                    listCallback = new List<Action<Entity>>();
                    this.prefabNameToListCallback.Add(name, listCallback);
                }
                listCallback.Add(callback);

                var spawnerEntity = this.EntityManager.CreateEntity(typeof(PrefabEntitySpawnerComponent));
                this.EntityManager.SetComponentData(spawnerEntity, new PrefabEntitySpawnerComponent()
                {
                    PrefabName = name,
                    Position   = position
                });

                if (!parent.Equals(default)) this.EntityManager.AddComponentData(spawnerEntity, new SetParentComponent() { ParentEntity = parent });
            }
        }
    }
}