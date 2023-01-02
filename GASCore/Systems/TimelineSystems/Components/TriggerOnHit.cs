namespace GASCore.Systems.TimelineSystems.Components
{
    using GASCore.Interfaces;
    using Unity.Collections;
    using Unity.Entities;

    public struct TriggerOnHit : IComponentData
    {
        public FixedString64Bytes FromAbilityEffectId;

        public class _ : ITriggerConditionActionConverter
        {
            public string FromAbilityEffectId;

            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
            {
                ecb.AddComponent(index, entity, new TriggerOnHit()
                {
                    FromAbilityEffectId = this.FromAbilityEffectId,
                });
            }
        }
    }

    public struct OnHitTargetElement : IBufferElementData
    {
        public Entity Target;
    }
}