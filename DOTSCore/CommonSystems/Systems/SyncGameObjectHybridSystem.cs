namespace DOTSCore.CommonSystems.Systems
{
    using System.Collections;
    using DOTSCore.CommonSystems.Components;
    using GameFoundation.Scripts.Utilities.Extension;
    using GameFoundation.Scripts.Utilities.ObjectPool;
    using Unity.Entities;
    using Unity.Transforms;
    using UnityEngine;
    using Zenject;

    [UpdateInGroup(typeof(PresentationSystemGroup), OrderLast = true)]
    public partial class SyncGameObjectHybridSystem : SystemBase
    {
        [Inject] private ObjectPoolManager                        objectPoolManager;
        private          BeginSimulationEntityCommandBufferSystem beginSimEcbSystem;
        protected override void OnCreate()
        {
            this.GetCurrentContainer().Inject(this);
            this.beginSimEcbSystem = this.World.GetExistingSystemManaged<BeginSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            //Create game object
            this.Entities
                .WithoutBurst()
                .WithStructuralChanges()
                .WithNone<GameObjectHybridLink, IsLoadingGameObjectTag>().ForEach((Entity entity, in AssetPathComponent assetPath, in Translation transform) =>
                {
                    this.GenerateGameObject(entity, assetPath, transform);
                }).Run();

            // Sync transform
            this.Entities.WithoutBurst().ForEach((in GameObjectHybridLink hybridLink, in LocalToWorld localToWorld, in Rotation rotation) =>
            {
                hybridLink.Object.transform.position = localToWorld.Position;
                hybridLink.Object.transform.rotation = rotation.Value;
            }).Run();

            //Sync view event
            this.Entities.WithoutBurst().ForEach((in EventQueue eventQueue, in ListenerCollector listenerCollector) =>
            {
                if (eventQueue.Value.Count <= 0) return;
                var valueCount = eventQueue.Value.Count;
                for (var i = 0; i < valueCount; i++)
                {
                    listenerCollector.Dispatch(eventQueue.Value.Dequeue());
                }
            }).Run();


            this.Entities.WithoutBurst().WithNone<AssetPathComponent>().ForEach((in GameObjectHybridLink hybridLink) =>
            {
                var destroyListener = hybridLink.Object.GetComponent<DestroyListener>();
                if (destroyListener == null)
                {
                    hybridLink.Object.Recycle();
                }
                else
                {
                    destroyListener.DestroyGameObject(hybridLink);
                }
            }).Run();

            var ecb = this.beginSimEcbSystem.CreateCommandBuffer().AsParallelWriter();
            this.Entities.WithAll<GameObjectHybridLink>().WithNone<AssetPathComponent>().ForEach((Entity entity, int entityInQueryIndex) =>
                {
                    ecb.RemoveComponent<GameObjectHybridLink>(entityInQueryIndex, entity);
                })
                .ScheduleParallel();
            this.beginSimEcbSystem.AddJobHandleForProducer(this.Dependency);
        }

        private async void GenerateGameObject(Entity entity, AssetPathComponent assetPath, Translation transform)
        {
            this.EntityManager.AddComponent<IsLoadingGameObjectTag>(entity);
            var tmpObject = await this.objectPoolManager.Spawn(assetPath.Path.Value, transform.Value);
            var animator  = tmpObject.GetComponent<Animator>();

            if (this.EntityManager.HasComponent<AssetPathComponent>(entity))
            {
                var listEntityViewMono = tmpObject.GetComponentsInChildren<IEntityViewMono>(true);
                foreach (var viewMono in listEntityViewMono)
                {
                    viewMono.BindEntity(this.EntityManager, entity);
                }

                var listViewMonoListener = tmpObject.GetComponentsInChildren<IViewMonoListener>(true);

                if (listViewMonoListener.Length > 0)
                {
                    var listenerCollector =  this.EntityManager.TryGetListenerCollector(entity);
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

                if (EntityManager.HasComponent<NonUniformScale>(entity))
                {
                    tmpObject.transform.localScale = EntityManager.GetComponentData<NonUniformScale>(entity).Value;
                }
            }
            else
            {
                tmpObject.Recycle();
            }
        }
    }
}