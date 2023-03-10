namespace GASCore.UnityHybrid.ViewMono
{
    using System;
    using System.Collections.Generic;
    using DOTSCore.CommonSystems.Components;
    using GASCore.Systems.StatSystems.Components;
    using GASCore.Systems.StatSystems.Systems;
    using Unity.Collections;
    using Unity.Entities;
    using UnityEngine;

    public abstract class BaseEntityStatView : MonoBehaviour, IEntityViewMono, IViewMonoListener
    {
        protected readonly Dictionary<FixedString64Bytes, StatDataElement>                StatData            = new();
        protected readonly Dictionary<FixedString64Bytes, Action<StatDataElement, float>> OnStatChangeActions = new();

        public virtual void BindEntity(EntityManager entityManager, Entity entity)
        {
            this.InitStatData(entityManager.GetBuffer<StatDataElement>(entity));
            this.InitOnStatChangeActions();
        }

        protected virtual void InitStatData(DynamicBuffer<StatDataElement> statDataBuffer)
        {
            this.StatData.Clear();
            foreach (var statData in statDataBuffer)
            {
                this.StatData[statData.StatName] = statData;
                this.InitStatView(statData);
            }
        }

        protected virtual void InitOnStatChangeActions()
        {
            this.OnStatChangeActions.Clear();
            foreach (var executor in this.GetComponentsInChildren<OnStatChangeExecutor>())
            {
                if (this.OnStatChangeActions.ContainsKey(executor.StatName))
                {
                    this.OnStatChangeActions[executor.StatName] += executor.Execute;
                }
                else
                {
                    this.OnStatChangeActions[executor.StatName] = executor.Execute;
                }
            }
        }

        public virtual void RegisterEvent(ListenerCollector listenerCollector)
        {
            listenerCollector.Subscribe<ChangeStatEvent>(this.OnEventTrigger);
        }

        protected virtual void OnEventTrigger(ChangeStatEvent data)
        {
            var changedStat  = data.ChangedStat;
            var statName     = changedStat.StatName;
            var changedValue = changedStat.CurrentValue - this.StatData[statName].CurrentValue;
            if (changedValue == 0) return;
            this.StatData[statName] = changedStat;
            this.ChangeStatView(changedStat, changedValue);
        }

        protected virtual void InitStatView(StatDataElement data)
        {
        }

        protected virtual void ChangeStatView(StatDataElement changedStat, float changedValue)
        {
            if (!this.OnStatChangeActions.TryGetValue(changedStat.StatName, out var action)) return;
            action(changedStat, changedValue);
        }

        protected StatDataElement GetCurrentStatData(FixedString64Bytes statName)
        {
            return this.StatData.TryGetValue(statName, out var stat) ? stat : default;
        }
    }

    public abstract class OnStatChangeExecutor : MonoBehaviour
    {
        public abstract FixedString64Bytes StatName { get; }
        public abstract void               Execute(StatDataElement changedStat, float changedValue);
    }
}