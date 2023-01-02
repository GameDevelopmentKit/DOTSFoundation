namespace GASCore.Systems.LogicEffectSystems.Components
{
    using GASCore.Interfaces;
    using Unity.Collections;
    using Unity.Entities;

    public struct RemoveEffectOnAffectedTarget : IComponentData
    {
        public FixedString64Bytes EffectId;
        public bool               IncludeIntendedToCreate;

        public class _ : IAbilityActionComponentConverter
        {
            public string EffectId;
            public bool   IncludeIntendedToCreate;
            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
            {
                ecb.AddComponent(index, entity, new RemoveEffectOnAffectedTarget()
                {
                    EffectId = this.EffectId,
                    IncludeIntendedToCreate = this.IncludeIntendedToCreate
                });
            }
        }
    }
}