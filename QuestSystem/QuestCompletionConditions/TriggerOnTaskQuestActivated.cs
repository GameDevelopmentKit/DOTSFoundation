namespace QuestSystem.QuestCompletionConditions
{
    using Unity.Collections;
    using Unity.Entities;

    public struct TriggerOnTaskQuestActivated : IComponentData
    {
        public FixedString64Bytes QuestSource;
        public int QuestId;
        public int TaskOrder; 
    }
}