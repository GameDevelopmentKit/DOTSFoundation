namespace GASCore.Systems.VisualEffectSystems.Systems
{
    using DOTSCore.CommonSystems.Components;
    using Gameplay.View.ViewMono;
    using GASCore.Groups;
    using GASCore.Systems.VisualEffectSystems.Components;
    using Unity.Burst;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityVisualEffectGroup))]
    [RequireMatchingQueriesForUpdate]
    public partial class UpdateTargetProjectileSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            this.Entities.WithoutBurst().WithStructuralChanges().WithAll<CurveForward>().ForEach((Entity entity,in CurveForward curveForward,in GameObjectHybridLink gameObjectHybridLink) =>
            {
                gameObjectHybridLink.Value.GetComponent<TargetViewOfProjectile>().UpdateTargetPosition(curveForward.Destination);
            }).Run();
        }
    }
}