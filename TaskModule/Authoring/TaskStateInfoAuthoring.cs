namespace TaskModule.Authoring
{
    using System.Collections.Generic;
    using Sirenix.OdinInspector;
    using TaskModule.Actions;
    using TaskModule.TaskBase;
    using Unity.Entities;
    using UnityEngine;

    public class TaskStateInfoAuthoring : ITaskComponentConverter
    {
        public string Description;
        public bool AutoActiveOnStart;
        public bool IsOptional;
      
        [SerializeReference] public List<ITaskActionComponentConverter> OnActiveActions = new();
        [SerializeReference] public List<ITaskGoalComponentConverter>   TaskGoals       = new();

        [Space]
        public bool ActiveSiblingOnComplete;
        [ShowIf("ActiveSiblingOnComplete")]
        public int SiblingTaskOrder;

        public void Convert(EntityManager entityManager, Entity taskEntity)
        {
            if (this.IsOptional) entityManager.AddComponent<OptionalTag>(taskEntity);
            if (this.AutoActiveOnStart) entityManager.AddComponent<AutoActiveOnStartTag>(taskEntity);
            foreach (var action in this.OnActiveActions)
            {
                action.Convert(entityManager, taskEntity);
            }

            foreach (var goal in this.TaskGoals)
            {
                goal.Convert(entityManager, taskEntity);
            }
            
            if (this.ActiveSiblingOnComplete) entityManager.AddComponentData(taskEntity, new ActiveSiblingTaskOnComplete(){TaskOrder = this.SiblingTaskOrder});
        }
    }
}