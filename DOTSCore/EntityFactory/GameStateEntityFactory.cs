namespace DOTSCore.EntityFactory
{
    using DOTSCore.CommonSystems.Components;
    using DOTSCore.Extension;
    using Unity.Collections;
    using Unity.Entities;

    public class GameStateEntityFactory : BaseEntityFactory<FixedString64Bytes>
    {
        protected override void InitComponents(EntityManager entityManager, Entity gameStateEntity, FixedString64Bytes initGameState)
        {
            entityManager.AddComponentData(gameStateEntity, new CurrentGameState() { Value = initGameState });
            entityManager.AddComponent<PreviousGameState>(gameStateEntity);
            entityManager.AddEnableableComponentTag<RequestChangeGameState>(gameStateEntity);
            entityManager.SetName(gameStateEntity, "GameState");
        }
    }
}