namespace QuestSystem.QuestCompletionConditions
{
    using TaskModule.Authoring;
    using Unity.Entities;
    using UnityEngine;

    [RequireComponent(typeof(TaskAuthoring))]
    public class TriggerOnTaskActivatedAuthoring : MonoBehaviour
    {
        public string QuestSource;
        public int    QuestId;
        public int    TaskOrder;

        public class TriggerOnTaskActivatedBaker : Baker<TriggerOnTaskActivatedAuthoring>
        {
            public override void Bake(TriggerOnTaskActivatedAuthoring requirementAuthoring)
            {
                var taskEntity = this.GetEntity(TransformUsageFlags.Dynamic);
                this.AddComponent(taskEntity, new TriggerOnTaskQuestActivated()
                {
                    QuestSource = requirementAuthoring.QuestSource,
                    QuestId     = requirementAuthoring.QuestId,
                    TaskOrder   = requirementAuthoring.TaskOrder
                });
            }
        }
    }
}