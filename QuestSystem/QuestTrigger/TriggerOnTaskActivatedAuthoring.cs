namespace QuestSystem.QuestTrigger
{
    using TaskModule.TaskBase;
    using Unity.Entities;
    using UnityEngine;

    public class TriggerOnTaskActivatedAuthoring : MonoBehaviour
    {
        public string QuestSource;
        public int    QuestId;
        public int    TaskOrder;
    }

    public class TriggerOnTaskActivatedBaker : Baker<TriggerOnTaskActivatedAuthoring>
    {
        public override void Bake(TriggerOnTaskActivatedAuthoring requirementAuthoring)
        {
            var taskEntity = this.GetEntity(TransformUsageFlags.Dynamic);
            this.InitSimpleTaskBaseData(taskEntity);
            this.AddComponent(taskEntity, new TriggerOnTaskQuestActivated()
            {
                QuestSource = requirementAuthoring.QuestSource,
                QuestId     = requirementAuthoring.QuestId,
                TaskOrder   = requirementAuthoring.TaskOrder
            });
        }
    }
}