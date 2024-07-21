namespace TaskModule.TaskBase
{
    using System;
    using DOTSCore.Extension;
    using TaskModule.Actions;
    using TaskModule.ActiveRequirement;
    using Unity.Entities;
    using Unity.Transforms;

    public static class TaskExtension
    {
        public static void InitSimpleTaskBaseData(this IBaker baker, Entity taskEntity, int taskOrder, Entity taskContainerOwner = default)
        {
            baker.AddComponent(taskEntity, new TaskIndex() { Value = taskOrder });
            baker.AddComponent<TaskProgress>(taskEntity);
            baker.AddEnableableComponentTag<ActivatedTag>(taskEntity);
            baker.AddEnableableComponentTag<CompletedTag>(taskEntity);
            
            if (taskContainerOwner != default)
            {
                baker.SetBuffer<SubTaskEntity>(taskContainerOwner).Add(new SubTaskEntity() { Value = taskEntity });
                baker.AddComponent(taskEntity, new ContainerOwner() { Value                    = taskContainerOwner });
                baker.AddComponent<LocalToWorld>(taskEntity);
                baker.AddComponent(taskEntity, new Parent() { Value = taskContainerOwner });
            }
        }

        #region EntityCommandBuffer

        public static void InitTaskBaseData(this EntityCommandBuffer.ParallelWriter ecb, int index, Entity taskEntity, int taskOrder,
            Entity taskContainerOwner = default)
        {
            ecb.AddComponent(index, taskEntity, new TaskIndex() { Value = taskOrder });
            ecb.AddComponent<TaskProgress>(index, taskEntity);
            ecb.SetEnabled(index, taskEntity, false);
            ecb.AddEnableableComponentTag<ActivatedTag>(index, taskEntity);
            ecb.AddEnableableComponentTag<CompletedTag>(index, taskEntity);
            if (taskContainerOwner != default)
            {
                ecb.AppendToBuffer(index, taskContainerOwner, new SubTaskEntity() { Value = taskEntity });
                ecb.AddComponent(index, taskEntity, new ContainerOwner() { Value          = taskContainerOwner });
                ecb.AddComponent<LocalToWorld>(index, taskEntity);
                ecb.SetParent(index, taskEntity, taskContainerOwner);
            }
        }

        public static Entity CreateTaskEntity(this EntityCommandBuffer.ParallelWriter ecb, int index, int taskOrder, Entity taskContainerOwner = default)
        {
            var taskEntity = ecb.CreateEntity(index);
            ecb.InitTaskBaseData(index, taskEntity, taskOrder, taskContainerOwner);
            return taskEntity;
        }

        public static Entity CreateTaskContainerEntity(this EntityCommandBuffer.ParallelWriter ecb, int index, int taskOrder,
            int requireOptionalAmount = 0, Entity taskContainerOwner = default)
        {
            var taskContainerEntity = CreateTaskEntity(ecb, index, taskOrder, taskContainerOwner);
            ecb.AddComponent(index, taskContainerEntity, new TaskContainerSetting() { RequireOptionalAmount = requireOptionalAmount });
            ecb.AddEnableableComponentTag<OnSubTaskElementCompleted>(index, taskContainerEntity);
            ecb.AddBuffer<SubTaskEntity>(index, taskContainerEntity);
            return taskContainerEntity;
        }

        #endregion

        #region EntityManager

        public static Entity CreateTaskEntity(this EntityManager entityManager, int taskOrder, Entity taskContainerOwner = default)
        {
            var taskEntity = entityManager.CreateEntity();
            entityManager.InitTaskBaseData(taskEntity, taskOrder, taskContainerOwner);
            return taskEntity;
        }

        public static void InitTaskBaseData(this EntityManager entityManager, Entity taskEntity, int taskOrder, Entity taskContainerOwner = default)
        {
            entityManager.AddComponentData(taskEntity, new TaskIndex() { Value = taskOrder });
            entityManager.AddComponent<TaskProgress>(taskEntity);
            entityManager.SetEnabled(taskEntity, false);
            entityManager.AddEnableableComponentTag<ActivatedTag>(taskEntity);
            entityManager.AddEnableableComponentTag<CompletedTag>(taskEntity);
            if (taskContainerOwner != default)
            {
                entityManager.GetBuffer<SubTaskEntity>(taskContainerOwner).Add(new SubTaskEntity() { Value = taskEntity });
                entityManager.AddComponentData(taskEntity, new ContainerOwner() { Value                    = taskContainerOwner });
                entityManager.AddComponent<LocalToWorld>(taskEntity);
                entityManager.SetParent(taskEntity, taskContainerOwner);
            }
        }

        public static Entity CreateTaskContainerEntity(this EntityManager entityManager, int taskOrder, int requireOptionalAmount = 0, Entity taskContainerOwner = default)
        {
            var taskContainerEntity = CreateTaskEntity(entityManager, taskOrder, taskContainerOwner);
            entityManager.AddComponentData(taskContainerEntity, new TaskContainerSetting() { RequireOptionalAmount = requireOptionalAmount });
            entityManager.AddEnableableComponentTag<OnSubTaskElementCompleted>(taskContainerEntity);
            entityManager.AddBuffer<SubTaskEntity>(taskContainerEntity);
            return taskContainerEntity;
        }

        public static Entity CreateTaskContainerEntity(this EntityManager entityManager, int taskOrder, int subTaskAmount, Action<Entity> initTaskContainerFunc,
            Action<int, Entity> initSubTaskFunc, int requireOptionalAmount = 0, Entity taskContainerOwner = default)
        {
            var taskContainerEntity = CreateTaskContainerEntity(entityManager, taskOrder, requireOptionalAmount, taskContainerOwner);
            initTaskContainerFunc?.Invoke(taskContainerEntity);
            for (var i = 0; i < subTaskAmount; i++)
            {
                var subTaskEntity = CreateTaskEntity(entityManager, i, taskContainerEntity);
                initSubTaskFunc?.Invoke(i, subTaskEntity);
            }

            return taskContainerEntity;
        }

        public static Entity CreateTaskContainerEntityInSequence(this EntityManager entityManager, int taskOrder, int subTaskAmount, Action<Entity> initTaskContainerFunc,
            Action<int, Entity> initSubTaskFunc, int requireOptionalAmount = 0, Entity taskContainerOwner = default)
        {
            return CreateTaskContainerEntity(entityManager, taskOrder, subTaskAmount, initTaskContainerFunc, (i, subTaskEntity) =>
            {
                if (i == 0)
                    entityManager.AddComponent<AutoActiveOnStartTag>(subTaskEntity);
                if (i + 1 < subTaskAmount)
                    entityManager.AddComponentData(subTaskEntity, new ActiveSiblingTaskOnComplete() { TaskOrder = i + 1 });
                initSubTaskFunc?.Invoke(i, subTaskEntity);
            }, requireOptionalAmount, taskContainerOwner);
        }

        #endregion
    }
}