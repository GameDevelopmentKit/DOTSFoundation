namespace QuestSystem.QuestBase
{
    using Unity.Collections;
    using Unity.Entities;

    public struct QuestInfo : IComponentData
    {
        public int Id;
        public FixedString64Bytes QuestSource;
    }
    
    //Quest should contain 3 parts:
    //1. Requirements to active quest
    //2. Tasks
    //3. Rewards
    
}