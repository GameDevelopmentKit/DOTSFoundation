namespace TutorialSystem.Systems
{
    using TaskModule;
    using TaskModule.TaskBase;
    using TutorialSystem.Components;
    using TutorialSystem.Helper;
    using Unity.Entities;
    using UnityEngine;
    using UnityEngine.EventSystems;

    [UpdateInGroup(typeof(TaskPresentationSystemGroup))]
    public partial class WaitTapToScreenSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            if (Input.GetMouseButton(0))
            {
                foreach (var (waitTapToScreen, entity) in SystemAPI.Query<RefRW<TapAnyWhereToComplete>>().WithAll<TaskIndex, ActivatedTag>().WithDisabled<CompletedTag>().WithEntityAccess())
                {
                    if (waitTapToScreen.ValueRO.DelayTime > 0)
                    {
                        if (waitTapToScreen.ValueRO.NextEndTimeValue <= 0)
                        {
                            waitTapToScreen.ValueRW.NextEndTimeValue = SystemAPI.Time.ElapsedTime + waitTapToScreen.ValueRO.DelayTime;
                        }

                        if (SystemAPI.Time.ElapsedTime >= waitTapToScreen.ValueRO.NextEndTimeValue)
                        {
                            waitTapToScreen.ValueRW.NextEndTimeValue = -1;
                            SystemAPI.SetComponentEnabled<CompletedTag>(entity, true);
                        }
                    }
                    else
                    {
                        SystemAPI.SetComponentEnabled<CompletedTag>(entity, true);
                    }
                }
            }

            foreach (var (tapToGameObject, entity) in SystemAPI.Query<TapToGameObjectComplete>().WithAll<TaskIndex, ActivatedTag>().WithDisabled<CompletedTag>().WithChangeFilter<ActivatedTag>()
                         .WithEntityAccess())
            {
                if (!TutorialObjectCollection.GetObjectInstanceByPath(tapToGameObject.GameObjectPath, out var targetObject)) continue;
                var eventTriggerEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };

                var currentEventTrigger = targetObject.AddComponent<EventTrigger>();
                currentEventTrigger.triggers.Add(eventTriggerEntry);
                eventTriggerEntry.callback.AddListener(_ =>
                {
                    SystemAPI.SetComponentEnabled<CompletedTag>(entity, true);
                    Object.Destroy(currentEventTrigger);
                });
            }
        }
    }
}