namespace DOTSCore.CommonSystems.Components
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Unity.Collections;
    using Unity.Entities;
    using UnityEngine;
    using UnityEngine.Jobs;

    public class GameObjectHybridLink : ICleanupComponentData
    {
        public GameObject Value;
    }

    /// <summary>
    /// Similar to <see cref="TransformAccessArray"/> just for <see cref="GameObject"/>.
    /// This structure has similar usage like <see cref="EntityQueryExtensionsForTransformAccessArray.GetTransformAccessArray"/> with caching functionality.
    /// </summary>
    public struct HybridTransformAccessArray : IDisposable
    {
        TransformAccessArray transformAccessArray;
        int                  currentVersion;

        public void Update(ref EntityQuery entityQuery)
        {
            int version = entityQuery.GetCombinedComponentOrderVersion();
            if (version == this.currentVersion && this.transformAccessArray.isCreated)
                return;

            this.currentVersion = version;
            var componentArray       = entityQuery.ToComponentArray<GameObjectHybridLink>();
            var transformArray       = new Transform[componentArray.Length];
            
            var componentArrayLength = componentArray.Length;
            for (int i = 0; i < componentArrayLength; ++i)
            {
                transformArray[i] = componentArray[i].Value.transform;
            }

            if (this.transformAccessArray.isCreated)
                this.transformAccessArray.SetTransforms(transformArray);
            else
                this.transformAccessArray = new TransformAccessArray(transformArray);
        }

        public static implicit operator TransformAccessArray(HybridTransformAccessArray v) => v.transformAccessArray;

        public void Dispose()
        {
            if (this.transformAccessArray.isCreated)
                this.transformAccessArray.Dispose();
        }
    }


    public class AnimatorHybridLink : IComponentData
    {
        public Animator Value;

        public static implicit operator Animator(AnimatorHybridLink link)     => link.Value;
        public static implicit operator AnimatorHybridLink(Animator animator) => new() { Value = animator };
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