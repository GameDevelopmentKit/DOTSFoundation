namespace TutorialSystem.Manager
{
    using System.Collections.Generic;
    using GameFoundation.Scripts.Interfaces;

    public class TutorialLocalData : ILocalData
    {
        public readonly List<int> CompletedTutorialQuestIds = new();

        public void Init()
        {
            
        }
    }
}