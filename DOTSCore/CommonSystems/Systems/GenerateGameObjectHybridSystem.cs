namespace DOTSCore.CommonSystems.Systems
{
    using System.Collections.Generic;
    using DOTSCore.CommonSystems.Components;
    using GameFoundation.Scripts.Utilities.Extension;
    using GameFoundation.Scripts.Utilities.ObjectPool;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Transforms;
    using UnityEngine;
    using Zenject;

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(TransformSystemGroup))]
    [RequireMatchingQueriesForUpdate]
    public partial class GenerateGameObjectHybridSystem : SystemBase
    {
        [Inject] private ObjectPoolManager objectPoolManager;

        private Dictionary<FixedString128Bytes, string> assetPathToStringValue = new();

        protected override void OnCreate() { this.GetCurrentContainer().Inject(this); }

        protected override void OnUpdate()
        {
            //Create game object
            this.Entities
                .WithoutBurst()
                .WithStructuralChanges()
                .WithNone<GameObjectHybridLink, IsLoadingGameObjectTag>().ForEach((Entity entity, in AddressablePathComponent assetPath, in LocalToWorld transform) =>
                {
                    this.GenerateGameObject(entity, assetPath, transform);
                }).Run();
        }

        private unsafe Vector3 GetScaleFromLocalToWorld(LocalToWorld transform)
        {
            var mat = *(UnityEngine.Matrix4x4*)&transform;
            return mat.lossyScale;
        }

        private async void GenerateGameObject(Entity entity, AddressablePathComponent assetPath, LocalToWorld transform)
        {
            this.EntityManager.AddComponent<IsLoadingGameObjectTag>(entity);
            if (!this.assetPathToStringValue.ContainsKey(assetPath.Value))
            {
                this.assetPathToStringValue.Add(assetPath.Value, assetPath.Value.Value);
            }

            var tmpObject = await this.objectPoolManager.Spawn(this.assetPathToStringValue[assetPath.Value], transform.Position, transform.Rotation);

            if (this.EntityManager.HasComponent<AddressablePathComponent>(entity))
            {
                var intiGameObjectHybridLink = tmpObject.GetComponent<InitGameObjectHybridLink>();
                if (intiGameObjectHybridLink == null)
                {
                    intiGameObjectHybridLink       = tmpObject.AddComponent<InitGameObjectHybridLink>();
                    tmpObject.transform.localScale = this.GetScaleFromLocalToWorld(transform);
                }

                intiGameObjectHybridLink.Init(this.EntityManager, entity);
                this.EntityManager.RemoveComponent<IsLoadingGameObjectTag>(entity);
            }
            else
            {
                tmpObject.Recycle();
            }
        }
    }
}