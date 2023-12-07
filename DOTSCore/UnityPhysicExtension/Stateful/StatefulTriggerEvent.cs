using Unity.Entities;
using Unity.Assertions;

namespace Unity.Physics.Stateful
{
    using UnityEngine;

    // Trigger Event that can be stored inside a DynamicBuffer
    public struct StatefulTriggerEvent : IBufferElementData, IStatefulSimulationEvent<StatefulTriggerEvent>
    {
        public Entity EntityA    { get; set; }
        public Entity EntityB    { get; set; }
        public int    BodyIndexA { get; set; }
        public int    BodyIndexB { get; set; }

        public                   StatefulEventState State { get => this.state; set => this.state = value; }
        [SerializeField] private StatefulEventState state;

#if UNITY_PHYSICS_CUSTOM
        public ColliderKey        ColliderKeyA { get;               set; }
        public ColliderKey        ColliderKeyB { get;               set; }


        public StatefulTriggerEvent(TriggerEvent triggerEvent)
        {
            EntityA = triggerEvent.EntityA;
            EntityB = triggerEvent.EntityB;
            BodyIndexA = triggerEvent.BodyIndexA;
            BodyIndexB = triggerEvent.BodyIndexB;
            ColliderKeyA = triggerEvent.ColliderKeyA;
            ColliderKeyB = triggerEvent.ColliderKeyB;
            this.state = default;
        }

        public int CompareTo(StatefulTriggerEvent other) => ISimulationEventUtilities.CompareEvents(this, other);
#endif

        // Returns other entity in EntityPair, if provided with one
        public Entity GetOtherEntity(Entity entity)
        {
            Assert.IsTrue((entity == EntityA) || (entity == EntityB));
            return (entity == EntityA) ? EntityB : EntityA;
        }
    }
}