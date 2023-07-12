namespace GASCore.Systems.AbilityMainFlow.Components
{
    using GASCore.Interfaces;
    using Unity.Entities;

    public struct EndBattleComponent: IComponentData
    {
        public class _:IAbilityActionComponentConverter
        {
            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
            {
                ecb.AddComponent<EndBattleComponent>(index,entity);
            }
        }
    }
}