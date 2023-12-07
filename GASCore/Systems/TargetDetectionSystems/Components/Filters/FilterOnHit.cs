namespace GASCore.Systems.TargetDetectionSystems.Components.Filters
{
    using Sirenix.OdinInspector;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Physics.Stateful;

    public struct FilterOnHit : IComponentData
    {
        public bool               IsLocal;
        public FixedString64Bytes FromAbilityEffectId;
        public StatefulEventState StateType;

        public class Option : FindTargetAuthoring.IOptionConverter
        {
            public                     bool               IsLocal             = true;
            [ShowIf("IsLocal")] public string             FromAbilityEffectId = string.Empty;
            [EnumToggleButtons] public StatefulEventState StateType           = StatefulEventState.Enter;
            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
            {
                ecb.AddComponent(index, entity, new FilterOnHit()
                {
                    IsLocal             = this.IsLocal,
                    FromAbilityEffectId = this.FromAbilityEffectId,
                    StateType           = this.StateType
                });
                ecb.AddComponent<OverrideFindTargetTag>(index, entity);
                ecb.AddBuffer<CacheTriggerEventElement>(index, entity);
            }
        }
    }

    public struct CacheTriggerEventElement : IBufferElementData
    {
        public Entity SourceEntity;
        public Entity OtherEntity;
    }
}