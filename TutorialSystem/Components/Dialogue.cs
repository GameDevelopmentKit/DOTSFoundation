namespace TutorialSystem.Components
{
    using TaskModule.Authoring;
    using Unity.Entities;
    using UnityEngine;

    public class Dialogue : IComponentData,ITaskActiveActionComponentConverter
    {
        public string UIAddressablePath = "TutorialDialogueUI";
        public string Content;
        
        internal GameObject LoadedUIObject;
        public void Convert(EntityManager entityManager, Entity taskEntity) { entityManager.AddComponentData(taskEntity, this); }
    }
}