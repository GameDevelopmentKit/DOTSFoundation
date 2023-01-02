namespace GASCore.UnityHybrid.ViewMono
{
    using System.Collections.Generic;
    using DOTSCore.CommonSystems.Components;
    using GASCore.Systems.StatSystems.Components;
    using GASCore.Systems.StatSystems.Systems;
    using Unity.Collections;
    using Unity.Entities;
    using UnityEngine;

    public abstract class BaseEntityStatView : MonoBehaviour, IEntityViewMono, IViewMonoListener
    {
        private Dictionary<FixedString64Bytes, StatDataElement> currentStatData = new();

        public void BindEntity(EntityManager entityManager, Entity entity) { this.InitStatData(entityManager.GetBuffer<StatDataElement>(entity)); }

        public void InitStatData(DynamicBuffer<StatDataElement> newStatData)
        {
            this.currentStatData.Clear();
            foreach (var statDataElement in newStatData)
            {
                this.currentStatData.Add(statDataElement.StatName, statDataElement);
                this.InitStatView(statDataElement);
            }
        }

        public virtual void RegisterEvent(ListenerCollector listenerCollector) { listenerCollector.Subscribe<ChangeStatEvent>(this.OnEventTrigger); }

        public void OnEventTrigger(ChangeStatEvent data)
        {
            var changeValue = data.ChangedStat.CurrentValue - this.currentStatData[data.ChangedStat.StatName].CurrentValue;
            if (changeValue != 0)
            {
                this.ChangeStatView(changeValue, data.ChangedStat);
                this.currentStatData[data.ChangedStat.StatName] = data.ChangedStat;
            }
        }

        protected virtual void InitStatView(StatDataElement data)                      { }
        protected virtual void ChangeStatView(float changeValue, StatDataElement data) { }
    }
}