namespace GASCore.Systems.CasterSystems.Components
{
    using Unity.Collections;
    using Unity.Entities;

    public struct RequestAddOrUpgradeAbility : IBufferElementData
    {
        public   FixedString64Bytes AbilityId { get;}
        public   int                Level     { get; }
        public   bool               IsPrefab  { get; }
        internal FixedString64Bytes AbilityLevelKey;
        public RequestAddOrUpgradeAbility(FixedString64Bytes abilityId, int level, bool isPrefab = false)
        {
            this.AbilityId       = abilityId;
            this.Level           = level;
            this.IsPrefab        = isPrefab;
            this.AbilityLevelKey = $"{this.AbilityId}_{this.Level}";
        }
    }
}