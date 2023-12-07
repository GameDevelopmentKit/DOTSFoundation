namespace TutorialSystem.Components
{
    using TaskModule.Authoring;
    using Unity.Entities;

    public struct TapAnyWhereToComplete : IComponentData, ITaskGoalComponentConverter
    {
        public void Convert(EntityManager entityManager, Entity taskEntity) { entityManager.AddComponentData(taskEntity, this); }
    }
    
    public class TapToGameObjectComplete : IComponentData, ITaskGoalComponentConverter
    {
        public string GameObjectPath;
        public void Convert(EntityManager entityManager, Entity taskEntity) { entityManager.AddComponentData(taskEntity, this); }
    }
}