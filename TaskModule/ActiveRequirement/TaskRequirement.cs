namespace TaskModule.ActiveRequirement
{
    using System.Collections.Generic;
    using TaskModule.Authoring;
    using Unity.Entities;

    public struct AutoActiveOnStartTag : IComponentData, ITaskRequirementComponentConverter
    {
        public void Convert(EntityManager entityManager, Entity taskEntity) { entityManager.AddComponent<AutoActiveOnStartTag>(taskEntity); }
    }

    public struct ActivateByCompletedTask : IBufferElementData
    {
        public int TaskIndex;

        public class _ : ITaskRequirementComponentConverter
        {
            public List<int> TaskIndexList;
            public void Convert(EntityManager entityManager, Entity taskEntity)
            {
                var buffer = entityManager.GetBuffer<ActivateByCompletedTask>(taskEntity);
                foreach (var t in this.TaskIndexList)
                {
                    buffer.Add(new ActivateByCompletedTask() { TaskIndex = t });
                }
            }
        }
    }
}