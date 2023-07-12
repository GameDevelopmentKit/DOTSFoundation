namespace GASCore.Systems.LogicEffectSystems.Components
{
    using Unity.Collections;
    using Unity.Entities;

    public struct EnemyId : IComponentData
    {
        public FixedString64Bytes EnemyID;
    }
}