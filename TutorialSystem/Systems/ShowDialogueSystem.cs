namespace TutorialSystem.Systems
{
    using GameFoundation.Scripts.UIModule.ScreenFlow.Managers;
    using GameFoundation.Scripts.Utilities.Extension;
    using GameFoundation.Scripts.Utilities.ObjectPool;
    using TaskModule;
    using TaskModule.TaskBase;
    using TutorialSystem.Components;
    using TutorialSystem.Helper;
    using TutorialSystem.View;
    using Unity.Entities;
    using UnityEngine;
    using Zenject;

    [UpdateInGroup(typeof(TaskSimulationSystemGroup))]
    [UpdateAfter(typeof(HighlightUIAreaSystem))]
    [RequireMatchingQueriesForUpdate]
    public partial class ShowDialogueSystem : SystemBase
    {
        [Inject] private ObjectPoolManager objectPool;
        [Inject] private IScreenManager    screenManager;


        protected override void OnCreate()
        {
            base.OnCreate();
            this.GetCurrentContainer()?.Inject(this);
        }

        protected override void OnUpdate()
        {
            // show dialogue on activated
            foreach (var dialogue in SystemAPI.Query<Dialogue>().WithAll<TaskIndex, ActivatedTag>().WithNone<CompletedTag>().WithChangeFilter<ActivatedTag>())
            {
                this.InstantiateDialogueUI(dialogue);
            }

            // remove dialogue on completed
            foreach (var dialogue in SystemAPI.Query<Dialogue>().WithAll<TaskIndex, CompletedTag>().WithChangeFilter<CompletedTag>())
            {
                if (dialogue.LoadedUIObject != null) dialogue.LoadedUIObject.Recycle();
            }
        }

        private async void InstantiateDialogueUI(Dialogue dialogue)
        {
            var dialogueUI = await this.objectPool.Spawn(dialogue.UIAddressablePath);
            dialogue.LoadedUIObject = dialogueUI;
            dialogueUI.transform.SetParent(this.screenManager.CurrentOverlayRoot, false);
            dialogueUI.transform.SetAsLastSibling();
            dialogueUI.GetComponent<RectTransform>().SetSize(0,0);
            dialogueUI.transform.localScale = Vector3.one;
            dialogueUI.GetComponent<TutorialDialogueUI>().SetContent(dialogue.Content);
        }
    }
}