namespace DOTSCore.CommonSystems.Systems
{
    using DOTSCore.CommonSystems.Components;
    using GameFoundation.Scripts.Utilities.Extension;
    using GameFoundation.Scripts.Utilities.ObjectPool;
    using Unity.Entities;
    using Unity.Transforms;
    using UnityEngine;
    using Zenject;

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [RequireMatchingQueriesForUpdate]
    public partial class GenerateGameObjectHybridSystem : SystemBase
    {
        [Inject] private ObjectPoolManager objectPoolManager;

        protected override void OnCreate() { this.GetCurrentContainer().Inject(this); }

        protected override void OnUpdate()
        {
            //Create game object
            this.Entities
                .WithoutBurst()
                .WithStructuralChanges()
                .WithNone<GameObjectHybridLink, IsLoadingGameObjectTag>().ForEach((Entity entity, in AssetPathComponent assetPath, in WorldTransform transform) =>
                {
                    this.GenerateGameObject(entity, assetPath, transform);
                }).Run();
        }

        private async void GenerateGameObject(Entity entity, AssetPathComponent assetPath, WorldTransform transform)
        {
            this.EntityManager.AddComponent<IsLoadingGameObjectTag>(entity);
            var tmpObject = await this.objectPoolManager.Spawn(assetPath.Path.Value, transform.Position, transform.Rotation);
            var animator  = tmpObject.GetComponent<Animator>();

            if (this.EntityManager.HasComponent<AssetPathComponent>(entity))
            {
                tmpObject.transform.localScale = new Vector3(transform.Scale, transform.Scale, transform.Scale);
                var listEntityViewMono = tmpObject.GetComponentsInChildren<IEntityViewMono>(true);
                foreach (var viewMono in listEntityViewMono)
                {
                    viewMono.BindEntity(this.EntityManager, entity);
                }

                var listViewMonoListener = tmpObject.GetComponentsInChildren<IViewMonoListener>(true);

                if (listViewMonoListener.Length > 0)
                {
                    var listenerCollector = this.EntityManager.TryGetListenerCollector(entity);
                    foreach (var viewMonoListener in listViewMonoListener)
                    {
                        viewMonoListener.RegisterEvent(listenerCollector);
                    }
                }

                this.EntityManager.AddComponentData(entity, new GameObjectHybridLink
                {
                    Object   = tmpObject,
                    Animator = animator,
                });

                this.EntityManager.RemoveComponent<IsLoadingGameObjectTag>(entity);
            }
            else
            {
                tmpObject.Recycle();
            }
        }
    }
}