namespace TaskModule.Authoring
{
    using TaskModule.Actions;
    using Unity.Entities;

    public class ActiveSiblingTaskOnCompleteAuthoring : ITaskComponentConverter
    {
        public int TaskOrder;
        public void Convert(EntityManager entityManager, Entity taskEntity)
        {
            entityManager.AddComponentData(taskEntity, new ActiveSiblingTaskOnComplete(){TaskOrder = this.TaskOrder});
        }
    }
}