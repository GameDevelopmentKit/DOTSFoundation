namespace GASCore.Systems.TimelineSystems.Components
{
    using GASCore.Interfaces;
    using Sirenix.OdinInspector;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Physics.Stateful;

    public struct TriggerOnHit : IComponentData
    {
        public FixedString64Bytes FromAbilityEffectId;
        public bool               IsDestroyAbilityEffectOnHit;
        public StatefulEventState StateType;
        
        public class _ : ITriggerConditionActionConverter
        {
            public string FromAbilityEffectId;
            public bool   IsDestroyAbilityEffectOnHit;
            [EnumToggleButtons]
            public StatefulEventState StateType = StatefulEventState.Enter;
            
            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
            {
                ecb.AddComponent(index, entity, new TriggerOnHit()
                {
                    FromAbilityEffectId = this.FromAbilityEffectId,
                    IsDestroyAbilityEffectOnHit = this.IsDestroyAbilityEffectOnHit,
                    StateType = this.StateType
                });
            }
        }
    }
}