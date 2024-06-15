namespace TutorialSystem.Manager
{
    using System.Linq;
    using DataManager.LocalData;
    using DOTSCore.Extension;
    using GameFoundation.Scripts.Utilities.Extension;
    using QuestSystem.QuestBase;
    using TaskModule;
    using TaskModule.Actions;
    using TaskModule.TaskBase;
    using TutorialSystem.Blueprint;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Transforms;
    using Zenject;

    public struct TutorialQuestTag : IComponentData { }

    [UpdateInGroup(typeof(TaskInitializeSystemGroup))]
    [RequireMatchingQueriesForUpdate]
    public partial class InitializeTutorialSystem : SystemBase
    {
        [Inject] IHandleLocalDataServices handleUserDataServices;

        private          TutorialLocalData  localData;
        private          TutorialBlueprint  tutorialBlueprint;
        private readonly FixedString64Bytes tutorialQuestSource = "Tutorial";
        protected override async void OnCreate()
        {
            this.GetCurrentContainer().Inject(this);
            this.tutorialBlueprint = TutorialBlueprint.Current;
            this.localData         = await this.handleUserDataServices.Load<TutorialLocalData>();

            if (!this.tutorialBlueprint.EnableFTUE) return;

            // get remaining tutorial
            var completedTutorial = this.localData.CompletedTutorialQuestIds.ToHashSet();
            var remainingTutorial = this.tutorialBlueprint.TutorialRecords.Where(record => !completedTutorial.Contains(record.ID)).ToList();
            if (remainingTutorial.Count == 0) return;

            // create tutorial root entity
            var tutorialRootEntity = EntityManager.CreateTaskContainerEntity(0);
            this.EntityManager.AddComponent<AutoActiveOnStartTag>(tutorialRootEntity);
            this.EntityManager.SetName(tutorialRootEntity, "TutorialRoot");

            // create tutorial quest entities
            for (var index = 0; index < remainingTutorial.Count; index++)
            {
                var record = remainingTutorial[index];
                if (completedTutorial.Contains(record.ID)) continue;
                var questTutEntity = this.EntityManager.CreateTaskContainerEntity(index, record.Tasks.Count, questTutEntity =>
                {
                    this.EntityManager.InitQuestData(questTutEntity, this.tutorialQuestSource, record.ID, false);
                    this.EntityManager.AddComponent<TutorialQuestTag>(questTutEntity);
                    this.EntityManager.AddComponent<LocalToWorld>(questTutEntity);
                    this.EntityManager.SetParent(questTutEntity, tutorialRootEntity);
                    this.EntityManager.SetName(questTutEntity, record.Name);
                }, (i, subTaskEntity) =>
                {
                    this.EntityManager.SetName(subTaskEntity, $"Task_{i}");
                    foreach (var component in record.Tasks[i].TaskEntity)
                    {
                        component.Convert(this.EntityManager, subTaskEntity);
                    }
                }, 0, tutorialRootEntity);

                if (index == 0)
                    this.EntityManager.AddComponent<AutoActiveOnStartTag>(questTutEntity);
                if (index + 1 < remainingTutorial.Count)
                    this.EntityManager.AddComponentData(questTutEntity, new ActiveSiblingTaskOnComplete() { TaskOrder = index + 1 });
            }
        }
        protected override void OnUpdate()
        {
            // cache completed tutorial to quest journal
            foreach (var questInfo in SystemAPI.Query<QuestInfo>().WithAll<TutorialQuestTag, CompletedTag>().WithChangeFilter<CompletedTag>())
            {
                this.localData.CompletedTutorialQuestIds.Add(questInfo.Id);
            }
        }
    }
}