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
                .WithNone<GameObjectHybridLink, IsLoadingGameObjectTag>().ForEach((Entity entity, in AssetPathComponent assetPath, in LocalToWorld transform) =>
                {
                    this.GenerateGameObject(entity, assetPath, transform);
                }).Run();
        }

        private unsafe Vector3 GetScaleFromLocalToWorld(LocalToWorld transform)
        {
            var mat = *(UnityEngine.Matrix4x4*) &transform;
            return mat.lossyScale;
        }

        private async void GenerateGameObject(Entity entity, AssetPathComponent assetPath, LocalToWorld transform)
        {
            this.EntityManager.AddComponent<IsLoadingGameObjectTag>(entity);
            var tmpObject = await this.objectPoolManager.Spawn(assetPath.Path.Value, transform.Position, transform.Rotation);

            if (this.EntityManager.HasComponent<AssetPathComponent>(entity))
            {
                tmpObject.transform.localScale = this.GetScaleFromLocalToWorld(transform);
                var listEntityViewMono = tmpObject.GetComponentsInChildren<IEntityViewMono>();
                foreach (var viewMono in listEntityViewMono)
                {
                    viewMono.BindEntity(this.EntityManager, entity);
                }

                var listViewMonoListener = tmpObject.GetComponentsInChildren<IViewMonoListener>();

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
                    Value   = tmpObject
                });

                var animator = tmpObject.GetComponent<Animator>();
                if (animator != null)
                {
                    this.EntityManager.AddComponentData(entity, new AnimatorHybridLink()
                    {
                        Value = animator
                    });
                }
                
                this.EntityManager.RemoveComponent<IsLoadingGameObjectTag>(entity);
            }
            else
            {
                tmpObject.Recycle();
            }
        }
    }
}