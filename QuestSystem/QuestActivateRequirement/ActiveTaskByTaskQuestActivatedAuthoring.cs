namespace QuestSystem.QuestActivateRequirement
{
    using TaskModule.Authoring;
    using Unity.Collections;
    using Unity.Entities;
    using UnityEngine;


    [RequireComponent(typeof(TaskAuthoring))]
    public class ActiveTaskByTaskQuestActivatedAuthoring : MonoBehaviour
    {
        public FixedString64Bytes QuestSource;
        public int                QuestId;
        public int                TaskOrder;

        public class ActiveTaskByTaskQuestActivatedBaker : Baker<ActiveTaskByTaskQuestActivatedAuthoring>
        {
            public override void Bake(ActiveTaskByTaskQuestActivatedAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new ActiveTaskByTaskQuestActivated { QuestSource = authoring.QuestSource, QuestId = authoring.QuestId, TaskOrder = authoring.TaskOrder });
            }
        }
    }
}