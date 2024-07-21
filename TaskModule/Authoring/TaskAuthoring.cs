namespace TaskModule.Authoring
{
    using DOTSCore.Extension;
    using TaskModule.TaskBase;
    using Unity.Entities;
    using UnityEngine;

    public class TaskAuthoring : MonoBehaviour
    {
        private class TaskAuthoringBaker : Baker<TaskAuthoring>
        {
            public override void Bake(TaskAuthoring authoring)
            {
                var taskEntity = this.GetEntity(TransformUsageFlags.Dynamic);
                this.AddComponent(taskEntity, new TaskIndex() { Value = 0 });
                if (this.IsActive())
                {
                    this.AddComponent<Disabled>(taskEntity);
                }
                this.AddComponent<TaskProgress>(taskEntity);
                this.AddEnableableComponentTag<ActivatedTag>(taskEntity);
                this.AddEnableableComponentTag<CompletedTag>(taskEntity);
            }
        }
    }
}