namespace TaskModule.Authoring
{
    using System.Collections.Generic;
    using DOTSCore.Extension;
    using TaskModule.TaskBase;
    using Unity.Entities;
    using UnityEngine;

    public class TaskContainerAuthoring : TaskBaseInfoAuthoring
    {
        [Space] public int                  RequireOptionalAmount;
        public         List<TaskEntityData> SubTasks = new();

        public override void Convert(EntityManager entityManager, Entity taskContainerEntity)
        {
            base.Convert(entityManager, taskContainerEntity);
            entityManager.AddComponentData(taskContainerEntity, new TaskContainerSetting() { RequireOptionalAmount = this.RequireOptionalAmount });
            entityManager.AddEnableableComponentTag<OnSubTaskElementCompleted>(taskContainerEntity);
            entityManager.AddBuffer<SubTaskEntity>(taskContainerEntity);

            for (var i = 0; i < this.SubTasks.Count; i++)
            {
                var subTaskEntity = entityManager.CreateTaskEntity(i, taskContainerEntity);
                entityManager.SetName(subTaskEntity, $"SubTask_{i}");
                foreach (var component in this.SubTasks[i].TaskEntity)
                {
                    component.Convert(entityManager, subTaskEntity);
                }
            }
        }
    }
}