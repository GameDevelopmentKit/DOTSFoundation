namespace GASCore.Systems.LogicEffectSystems.Systems
{
    using DOTSCore.Extension;
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityLogicEffectGroup))]
    [RequireMatchingQueriesForUpdate]
    public partial class DeathSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithoutBurst().WithStructuralChanges().WithChangeFilter<DeathComponent>().ForEach((in AffectedTargetComponent affectedTarget) =>
            { 
                EntityManager.AddComponentData(affectedTarget.Value,new DeathComponent());
            }).Run();
        }
    }
}