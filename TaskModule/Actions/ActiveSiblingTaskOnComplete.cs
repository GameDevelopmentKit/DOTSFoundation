namespace TaskModule.Actions
{
    using TaskModule.Authoring;
    using Unity.Entities;

    public struct ActiveSiblingTaskOnComplete : IComponentData, ITaskCompleteActionComponentConverter
    {
        public int  TaskOrder;
        public void Convert(EntityManager entityManager, Entity taskEntity)
        {
            entityManager.AddComponentData(taskEntity, new ActiveSiblingTaskOnComplete() { TaskOrder = this.TaskOrder });
        }
    }
}