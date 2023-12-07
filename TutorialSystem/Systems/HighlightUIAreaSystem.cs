namespace TutorialSystem.Systems
{
    using GameFoundation.Scripts.UIModule.ScreenFlow.Managers;
    using GameFoundation.Scripts.Utilities.Extension;
    using GameFoundation.Scripts.Utilities.ObjectPool;
    using TaskModule;
    using TaskModule.TaskBase;
    using TutorialSystem.Components;
    using TutorialSystem.Helper;
    using Unity.Entities;
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using Zenject;

    [UpdateInGroup(typeof(TaskSimulationSystemGroup))]
    [RequireMatchingQueriesForUpdate]
    public partial class HighlightUIAreaSystem : SystemBase
    {
        [Inject] private ObjectPoolManager objectPool;
        [Inject] private IScreenManager    screenManager;

        private GameObject TutorialDarkMask;

        protected override void OnCreate()
        {
            base.OnCreate();
            this.GetCurrentContainer()?.Inject(this);
        }

        protected override void OnUpdate()
        {
            foreach (var highlightUIAreaData in SystemAPI.Query<HighlightUIArea>().WithAll<TaskIndex, ActivatedTag>().WithNone<CompletedTag>().WithChangeFilter<ActivatedTag>())
            {
                if (!TutorialObjectCollection.GetObjectInstanceByPath(highlightUIAreaData.GameObjectPath, out var targetObject)) continue;

                // setup force to target object
                if (highlightUIAreaData.IsForce)
                {
                    highlightUIAreaData.ForcedObject = new GameObjectWrapper(targetObject);
                    if (this.TutorialDarkMask == null)
                    {
                        this.TutorialDarkMask = Addressables.InstantiateAsync("TutorialDarkMask", Vector3.zero, Quaternion.identity).WaitForCompletion();
                        this.TutorialDarkMask.transform.SetParent(this.screenManager.CurrentOverlayRoot, false);
                    }

                    this.TutorialDarkMask.SetActive(true);
                    this.TutorialDarkMask.transform.SetAsLastSibling();
                    highlightUIAreaData.ForcedObject.SetNewParent(this.TutorialDarkMask.transform);
                }


                //setup highlight effects
                foreach (var effectInfo in highlightUIAreaData.Effects)
                {
                    this.SetupEffect(effectInfo, targetObject);
                }
            }

            //remove highlight effects on completed
            foreach (var highlightUIAreaData in SystemAPI.Query<HighlightUIArea>().WithAll<TaskIndex, CompletedTag>().WithChangeFilter<CompletedTag>())
            {
                if (highlightUIAreaData.IsForce)
                {
                    if (this.TutorialDarkMask != null) this.TutorialDarkMask.SetActive(false);
                    highlightUIAreaData.ForcedObject?.SetOriginParent();
                }


                //setup highlight effects
                foreach (var effectInfo in highlightUIAreaData.Effects)
                {
                    //todo check exception here
                    if (effectInfo.LoadedEffectObject != null) effectInfo.LoadedEffectObject.Recycle();
                }
            }
        }

        private async void SetupEffect(HighlightUIArea.EffectInfo effectInfo, GameObject targetObject)
        {
            var effectObject = await this.objectPool.Spawn(effectInfo.EffectAddressablePath);

            if (effectObject == null) return;

            effectInfo.LoadedEffectObject = effectObject;
            var effectObjTransform = effectObject.GetComponent<RectTransform>();
            effectObjTransform.SetParent(targetObject.transform, false);

            if (effectInfo.IsFistSiblingInTransform) effectObjTransform.SetAsFirstSibling();
            effectObjTransform.SetAnchor(effectInfo.EffectAnchor).SetAnchoredPosition(effectInfo.Offset);
            effectObjTransform.localScale = Vector3.one;
        }
    }
}