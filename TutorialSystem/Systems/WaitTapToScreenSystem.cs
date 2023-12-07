namespace TutorialSystem.Systems
{
    using TaskModule;
    using TaskModule.TaskBase;
    using TutorialSystem.Components;
    using TutorialSystem.Helper;
    using Unity.Entities;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.InputSystem;

    [UpdateInGroup(typeof(TaskPresentationSystemGroup))]
    public partial class WaitTapToScreenSystem : SystemBase
    {
        private InputAction tapAction;
        protected override void OnCreate()
        {
            base.OnCreate();
            tapAction           =  new InputAction("Tap", InputActionType.PassThrough, "<Pointer>/press");
            tapAction.performed += OnTap;
            tapAction.Enable();
        }
        private void OnTap(InputAction.CallbackContext input)
        {
            Debug.Log("Screen tapped at position: " + Mouse.current.position.ReadValue());
            if (!input.performed) return;
            foreach (var (waitTapToScreen, entity) in SystemAPI.Query<TapAnyWhereToComplete>().WithAll<TaskIndex, ActivatedTag>().WithDisabled<CompletedTag>().WithEntityAccess())
            {
                SystemAPI.SetComponentEnabled<CompletedTag>(entity, true);
            }
        }
        
        protected override void OnUpdate()
        {
            foreach (var (tapToGameObject, entity) in SystemAPI.Query<TapToGameObjectComplete>().WithAll<TaskIndex, ActivatedTag>().WithDisabled<CompletedTag>().WithChangeFilter<ActivatedTag>().WithEntityAccess())
            {
                if (!TutorialObjectCollection.GetObjectInstanceByPath(tapToGameObject.GameObjectPath, out var targetObject)) continue;
                var eventTriggerEntry = new EventTrigger.Entry {eventID = EventTriggerType.PointerClick};
                
                var currentEventTrigger = targetObject.AddComponent<EventTrigger>();
                currentEventTrigger.triggers.Add(eventTriggerEntry);
                eventTriggerEntry.callback.AddListener(_ =>
                {
                    SystemAPI.SetComponentEnabled<CompletedTag>(entity, true);
                    Object.Destroy(currentEventTrigger);
                });
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            tapAction.performed -= OnTap;
            tapAction.Disable();
        }
    }
}