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
                var destroyListener = hybridLink.Value.GetComponent<IDestroyListener>();
                if (destroyListener == null)
                {
                    hybridLink.Value.Recycle();
                }
                else
                {
                    destroyListener.DestroyGameObject();
                }

                EntityManager.RemoveComponent<GameObjectHybridLink>(entity);
            }).Run();
        }
    }
}