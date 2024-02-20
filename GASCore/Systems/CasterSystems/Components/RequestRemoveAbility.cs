namespace GASCore.Systems.CasterSystems.Components
{
    using Unity.Collections;
    using Unity.Entities;

    public struct RequestRemoveAbility : IBufferElementData
    {
        public   FixedString64Bytes AbilityId { get; }
        public   int                Level   { get; }

        public RequestRemoveAbility(FixedString64Bytes abilityId, int level)
        {
            this.AbilityId       = abilityId;
            this.Level         = level;
        }
        
        public RequestRemoveAbility(FixedString64Bytes abilityId)
        {
            this.AbilityId = abilityId;
            this.Level     = -1;
        }
    }
}