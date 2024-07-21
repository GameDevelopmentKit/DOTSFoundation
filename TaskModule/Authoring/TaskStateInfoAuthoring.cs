namespace TaskModule.Authoring
{
    using System.Collections.Generic;
    using Sirenix.OdinInspector;
    using TaskModule.Actions;
    using TaskModule.ActiveRequirement;
    using TaskModule.TaskBase;
    using Unity.Entities;
    using UnityEngine;

    public class TaskBaseInfoAuthoring : ITaskInfoConverter
    {
        public string Description;
        public bool   IsOptional;

        [Space] [SerializeReference] public ITaskRequirementComponentConverter TaskRequirement;

        [Space] [PropertyOrder(1000)] [SerializeReference]
        public List<ITaskCompleteActionComponentConverter> OnCompleteActions = new();

        public virtual void Convert(EntityManager entityManager, Entity taskEntity)
        {
            if (this.IsOptional) entityManager.AddComponent<OptionalTag>(taskEntity);
            this.TaskRequirement?.Convert(entityManager, taskEntity);
            foreach (var action in this.OnCompleteActions)
            {
                action.Convert(entityManager, taskEntity);
            }
        }
    }

    public class TaskStateInfoAuthoring : TaskBaseInfoAuthoring
    {
        [SerializeReference]         public List<ITaskActiveActionComponentConverter> OnActiveActions = new();
        [Space] [SerializeReference] public ITaskGoalComponentConverter               TaskGoal;

        public override void Convert(EntityManager entityManager, Entity taskEntity)
        {
            base.Convert(entityManager, taskEntity);
            foreach (var action in this.OnActiveActions)
            {
                action.Convert(entityManager, taskEntity);
            }

            this.TaskGoal?.Convert(entityManager, taskEntity);
        }
    }
}