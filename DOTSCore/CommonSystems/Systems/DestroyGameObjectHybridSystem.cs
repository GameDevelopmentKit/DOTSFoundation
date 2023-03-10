namespace DOTSCore.CommonSystems.Systems
{
    using DOTSCore.CommonSystems.Components;
    using GameFoundation.Scripts.Utilities.ObjectPool;
    using Unity.Entities;

    [UpdateInGroup(typeof(PresentationSystemGroup), OrderLast = true)]
    [RequireMatchingQueriesForUpdate]
    public partial class DestroyGameObjectHybridSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            this.Entities.WithoutBurst().WithStructuralChanges().WithNone<AssetPathComponent>().ForEach((Entity entity, in GameObjectHybridLink hybridLink) =>
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

                EntityManager.RemoveComponent<GameObjectHybridLink>(entity);
            }).Run();
        }
    }
}