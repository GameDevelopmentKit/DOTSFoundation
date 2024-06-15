namespace TutorialSystem.Manager
{
    using System.Collections.Generic;
    using DataManager.LocalData;

    public class TutorialLocalData : ILocalData
    {
        public readonly List<int> CompletedTutorialQuestIds = new();
    }
}