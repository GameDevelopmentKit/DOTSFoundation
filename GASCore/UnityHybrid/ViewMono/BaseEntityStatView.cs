namespace GASCore.UnityHybrid.ViewMono
{
    using System;
    using System.Collections.Generic;
    using DOTSCore.CommonSystems.Components;
    using GASCore.Systems.StatSystems.Components;
    using GASCore.Systems.StatSystems.Systems;
    using Sirenix.Utilities;
    using Unity.Collections;
    using Unity.Entities;
    using UnityEngine;

    public class BaseEntityStatView : MonoBehaviour, IEntityViewMono, IViewMonoListener
    {
        protected          EntityManager                                   entityManager;
        protected          Entity                                          entity;
        protected readonly Dictionary<FixedString64Bytes, StatDataElement> statsData = new();

        private readonly Dictionary<FixedString64Bytes, Action<StatDataElement>>        initStatViewActions   = new();
        private readonly Dictionary<FixedString64Bytes, Action<StatDataElement, float>> updateStatViewActions = new();

        public void BindEntity(EntityManager entityManager, Entity entity)
        {
            this.entityManager = entityManager;
            this.entity        = entity;
            this.InitStatsData();
            this.statsData.Values.ForEach(this.InitStatView);
        }

        private void InitStatsData()
        {
            this.statsData.Clear();
            this.initStatViewActions.Clear();
            this.updateStatViewActions.Clear();

            if(!this.entityManager.HasBuffer<StatDataElement>(this.entity)) return;
            var statDataBuffer = this.entityManager.GetBuffer<StatDataElement>(this.entity);

            foreach (var statData in statDataBuffer)
            {
                this.statsData[statData.StatName]             = statData;
                this.initStatViewActions[statData.StatName]   = null;
                this.updateStatViewActions[statData.StatName] = null;
            }

            foreach (var executor in this.GetComponentsInChildren<OnStatChangeExecutor>())
            {
                this.initStatViewActions[executor.StatName]   +=  executor.InitStatView;
                this.updateStatViewActions[executor.StatName] += executor.UpdateStatView;
            }
        }

        public void RegisterEvent(ListenerCollector listenerCollector)
        {
            listenerCollector.Subscribe<ChangeStatEvent>(this.OnStatChange);
        }

        protected virtual void OnStatChange(ChangeStatEvent data)
        {
            var stat         = data.ChangedStat;
            var statName     = stat.StatName;
            var changedValue = stat.CurrentValue - this.statsData[statName].CurrentValue;
            if (changedValue == 0) return;
            this.statsData[statName] = stat;
            this.UpdateStatView(stat, changedValue);
        }

        protected virtual void InitStatView(StatDataElement stat)
        {
            this.initStatViewActions[stat.StatName]?.Invoke(stat);
        }

        protected virtual void UpdateStatView(StatDataElement stat, float changedValue)
        {
            this.updateStatViewActions[stat.StatName]?.Invoke(stat, changedValue);
        }
    }

    public abstract class OnStatChangeExecutor : MonoBehaviour
    {
        public abstract FixedString64Bytes StatName { get; }
        public abstract void               InitStatView(StatDataElement stat);
        public abstract void               UpdateStatView(StatDataElement stat, float changedValue);
    }
}