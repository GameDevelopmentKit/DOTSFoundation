namespace DOTSCore.CommonSystems.Systems
{
    using DOTSCore.CommonSystems.Components;
    using GASCore.Systems.AbilityMainFlow.Components;
    using Unity.Entities;

    public partial class IgnoreSysnTransformSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            this.Entities.WithoutBurst().WithStructuralChanges().WithChangeFilter<IgnoreSysnTransformComponent>().ForEach((Entity entity,in IgnoreSysnTransformComponent ignoreSysnTransformComponent) =>
            {
                if (ignoreSysnTransformComponent.IsAddForAffectTarget)
                {
                    var affectTarget = EntityManager.GetComponentData<AffectedTargetComponent>(entity).Value;
                    EntityManager.AddComponentData(affectTarget, new IgnoreSysnTransformComponent());
                }
            }).Run();
        }
    }
}