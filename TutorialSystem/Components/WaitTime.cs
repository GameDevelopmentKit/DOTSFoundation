namespace TutorialSystem.Components
{
    using TaskModule.Authoring;
    using Unity.Entities;

    public struct WaitTime : IComponentData, ITaskGoalComponentConverter
    {
        public   float  Seconds;
        internal double NextEndTimeValue;
        public   void   Convert(EntityManager entityManager, Entity taskEntity) { entityManager.AddComponentData(taskEntity, this); }
    }
}