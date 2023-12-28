namespace TaskModule.Authoring
{
    using System;
    using System.Collections.Generic;
    using Unity.Entities;
    using UnityEngine;

    [Serializable]
    public class TaskEntityData
    {
        internal                    string                   TaskTitle  = "Task Order";
        [SerializeReference] public List<ITaskInfoConverter> TaskEntity = new();
    }

    public interface ITaskComponentConverter
    {
        public void Convert(EntityManager entityManager, Entity taskEntity);
    }

    public interface ITaskActionComponentConverter : ITaskComponentConverter { }

    public interface ITaskGoalComponentConverter : ITaskComponentConverter { }

    public interface ITaskInfoConverter : ITaskComponentConverter { }
}