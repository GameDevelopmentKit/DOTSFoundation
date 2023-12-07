namespace QuestSystem.QuestTrigger
{
    using Unity.Collections;
    using Unity.Entities;

    public struct TriggerOnTaskQuestCompleted : IComponentData
    {
        public FixedString64Bytes QuestSource;
        public int QuestId;
        public int TaskOrder; 
    }
}