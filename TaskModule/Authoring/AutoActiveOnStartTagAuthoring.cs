namespace TaskModule.Authoring
{
    using TaskModule.ActiveRequirement;
    using TaskModule.TaskBase;
    using Unity.Entities;

    public class AutoActiveOnStartTagAuthoring : ITaskComponentConverter
    {
        public void Convert(EntityManager entityManager, Entity taskEntity) { entityManager.AddComponent<AutoActiveOnStartTag>(taskEntity); }
    }
}