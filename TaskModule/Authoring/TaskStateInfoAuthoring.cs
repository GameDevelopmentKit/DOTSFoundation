namespace TaskModule.Authoring
{
    using System.Collections.Generic;
    using Sirenix.OdinInspector;
    using TaskModule.Actions;
    using TaskModule.TaskBase;
    using Unity.Entities;
    using UnityEngine;
    
    public class TaskBaseInfoAuthoring : ITaskInfoConverter
    {
        public string Description;

        [HorizontalGroup("Group1", DisableAutomaticLabelWidth = true)]
        public bool AutoActiveOnStart;

        [HorizontalGroup("Group1")] public bool IsOptional;

        [HorizontalGroup("Group2", Order = 1000, DisableAutomaticLabelWidth = true)] [Space]
        public bool ActiveSiblingOnComplete;

        [HorizontalGroup("Group2")] [ShowIf("ActiveSiblingOnComplete")] [Space]
        public int SiblingTaskOrder;

        public virtual void Convert(EntityManager entityManager, Entity taskEntity)
        {
            if (this.IsOptional) entityManager.AddComponent<OptionalTag>(taskEntity);
            if (this.AutoActiveOnStart) entityManager.AddComponent<AutoActiveOnStartTag>(taskEntity);

            if (this.ActiveSiblingOnComplete) entityManager.AddComponentData(taskEntity, new ActiveSiblingTaskOnComplete() { TaskOrder = this.SiblingTaskOrder });
        }
    }

    public class TaskStateInfoAuthoring : TaskBaseInfoAuthoring
    {
        [Space] [SerializeReference] public List<ITaskActionComponentConverter> OnActiveActions = new();
        [SerializeReference]         public List<ITaskGoalComponentConverter>   TaskGoals       = new();

        public override void Convert(EntityManager entityManager, Entity taskEntity)
        {
            base.Convert(entityManager, taskEntity);
            foreach (var action in this.OnActiveActions)
            {
                action.Convert(entityManager, taskEntity);
            }

            foreach (var goal in this.TaskGoals)
            {
                goal.Convert(entityManager, taskEntity);
            }
        }
    }
}