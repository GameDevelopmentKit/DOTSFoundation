namespace AdventureWorld.CombatSystem.AbilitySystemExtension.Components
{
    using GASCore.Systems.TargetDetectionSystems.Components;
    using Unity.Entities;

    public struct AimNearestAgentInsideCastRange : IComponentData
    {
        public class _ : BaseFindTargetAuthoring
        {
            public override void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
            {
                base.Convert(ecb, index, entity);
                ecb.AddComponent(index, entity, new AimNearestAgentInsideCastRange());
            }
        }
    }
}