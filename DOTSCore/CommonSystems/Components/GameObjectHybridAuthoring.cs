namespace DOTSCore.CommonSystems.Components
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Unity.Entities;
    using UnityEngine;

    public class GameObjectHybridLink : ICleanupComponentData
    {
        public GameObject Object;
        public Animator   Animator;
    }

    public struct IsLoadingGameObjectTag : IComponentData { }

    public class EventQueue : IComponentData
    {
        public Queue Value;
    }

    public class ListenerCollector : IComponentData
    {
        private readonly Dictionary<Type, List<Action<object>>> eventTypeToCallbackObject = new();
        private readonly Dictionary<Type, List<Action>>         eventTypeToCallback       = new();

        public void Subscribe<TEvent>(Action<TEvent> callback) => AddCallback<TEvent, Action<object>>(this.eventTypeToCallbackObject, args => callback((TEvent)args));

        public void Subscribe<TEvent>(Action callback) => AddCallback<TEvent, Action>(this.eventTypeToCallback, callback);

        public void Dispatch(object eventData)
        {
            if (this.eventTypeToCallbackObject.TryGetValue(eventData.GetType(), out var callbackObject))
            {
                foreach (var action in callbackObject)
                {
                    action.Invoke(eventData);
                }
            }
            else if (this.eventTypeToCallback.TryGetValue(eventData.GetType(), out var callback))
            {
                foreach (var action in callback)
                {
                    action.Invoke();
                }
            }
        }

        private void AddCallback<TType, TValue>(Dictionary<Type, List<TValue>> dict, TValue value)
        {
            if (!dict.TryGetValue(typeof(TType), out var listCallback))
            {
                listCallback = new List<TValue>();
                dict.Add(typeof(TType), listCallback);
            }

            listCallback.Add(value);
        }
    }

    public interface IEntityViewMono
    {
        void BindEntity(EntityManager entityManager, Entity entity);
    }

    public interface IViewMonoListener
    {
        public void RegisterEvent(ListenerCollector listenerCollector);
    }

    public static class EventViewExtension
    {
        public static void TryEnqueueViewEvent(this EntityManager entityManager, Entity target, object eventData)
        {
            if (entityManager.HasComponent<ListenerCollector>(target) && entityManager.HasComponent<EventQueue>(target))
            {
                entityManager.GetComponentData<EventQueue>(target).Value.Enqueue(eventData);
            }
        }

        public static ListenerCollector TryGetListenerCollector(this EntityManager entityManager, Entity entity)
        {
            if (entityManager.HasComponent<ListenerCollector>(entity))
            {
                return entityManager.GetComponentData<ListenerCollector>(entity);
            }
            else
            {
                entityManager.AddComponentData(entity, new EventQueue() { Value = new Queue() });
                var listenerCollector = new ListenerCollector();
                entityManager.AddComponentData(entity, listenerCollector);
                return listenerCollector;
            }
        }
    }
}