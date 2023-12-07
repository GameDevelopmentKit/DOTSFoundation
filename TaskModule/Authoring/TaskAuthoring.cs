namespace TaskModule.Authoring
{
    using TaskModule.TaskBase;
    using Unity.Entities;
    using UnityEngine;

    public class TaskAuthoring : MonoBehaviour
    {
        private class TaskAuthoringBaker : Baker<TaskAuthoring>
        {
            public override void Bake(TaskAuthoring authoring)
            {
                this.InitSimpleTaskBaseData(this.GetEntity(TransformUsageFlags.Dynamic));
            }
        }
    }
}