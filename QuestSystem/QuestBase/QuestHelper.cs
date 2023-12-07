namespace QuestSystem.QuestBase
{
    using System;
    using TaskModule.TaskBase;
    using Unity.Collections;
    using Unity.Entities;

    public static class QuestHelper
    {
        public static bool ContainTriggerQuestIdBuffer(this in NativeArray<QuestInfo> questIds, in DynamicBuffer<int> questRequireBuffer)
        {
            foreach (var activateByQuest in questRequireBuffer)
            {
                if (activateByQuest == -1) continue;
                if (questIds.Length == 0) return false;
                var isFound = false;
                foreach (var questCompletedId in questIds)
                {
                    if (questCompletedId.Id != activateByQuest) continue;
                    isFound = true;
                    break;
                }

                //if missing any quest require, return false
                if (!isFound) return false;
            }

            return true;
        }

        public static Entity CreateQuestEntityInSequence(this EntityManager entityManager, FixedString64Bytes questSource, int questId, int subTaskAmount, Action<int, Entity> initSubTaskFunc,
            bool activatedOnStart = false,
            int requireOptionalAmount = 0)
        {
            return entityManager.CreateTaskContainerEntityInSequence(questId, subTaskAmount,
                entity => { InitQuestData(entityManager, entity, questSource, questId, activatedOnStart); },
                initSubTaskFunc, requireOptionalAmount);
        }

        public static Entity CreateQuestEntity(this EntityManager entityManager, FixedString64Bytes questSource, int questId, int subTaskAmount, Action<int, Entity> initSubTaskFunc,
            bool activatedOnStart = false,
            int requireOptionalAmount = 0)
        {
            var questEntity = entityManager.CreateTaskContainerEntity(questId, subTaskAmount,
                entity => { InitQuestData(entityManager, entity, questSource, questId, activatedOnStart); },
                initSubTaskFunc, requireOptionalAmount);

            return questEntity;
        }

        public static Entity CreateQuestEntity(this EntityManager entityManager, FixedString64Bytes questSource, int questId, bool activatedOnStart = false,
            int requireOptionalAmount = 0)
        {
            var questEntity = entityManager.CreateTaskContainerEntity(questId, requireOptionalAmount);
            InitQuestData(entityManager, questEntity, questSource, questId, activatedOnStart);

            return questEntity;
        }
        public static void InitQuestData(this EntityManager entityManager, Entity questEntity, FixedString64Bytes questSource, int questId, bool activatedOnStart)
        {
            entityManager.AddComponentData(questEntity, new QuestInfo()
            {
                Id          = questId,
                QuestSource = questSource
            });

            if (activatedOnStart) entityManager.AddComponent<AutoActiveOnStartTag>(questEntity);
        }
    }
}