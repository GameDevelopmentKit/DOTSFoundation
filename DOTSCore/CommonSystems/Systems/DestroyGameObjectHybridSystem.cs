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
            this.Entities.WithoutBurst().WithStructuralChanges().WithNone<AddressablePathComponent>().ForEach((Entity entity, in GameObjectHybridLink hybridLink) =>
            {
                this.DestroyLinkGameObject(hybridLink, entity);
            }).Run();

            //todo find another solution to change addressable path
            // this.Entities.WithoutBurst().WithStructuralChanges().WithAll<SyncGameObjectTransformCleanup>().WithChangeFilter<AddressablePathComponent>().ForEach((Entity entity, in GameObjectHybridLink hybridLink) =>
            // {
            //     DestroyLinkGameObject(hybridLink, entity);
            //     EntityManager.RemoveComponent<SyncGameObjectTransformCleanup>(entity);
            // }).Run();
        }
        private void DestroyLinkGameObject(GameObjectHybridLink hybridLink, Entity entity)
        {
            var destroyListener = hybridLink.Value.GetComponent<IDestroyListener>();
            if (destroyListener == null)
            {
                hybridLink.Value.Recycle();
            }
            else
            {
                destroyListener.DestroyGameObject();
            }
            this.EntityManager.RemoveComponent<GameObjectHybridLink>(entity);
        }
    }
}