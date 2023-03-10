namespace GASCore.Systems.TargetDetectionSystems.Components
{
    using GASCore.Interfaces;
    using Unity.Collections;
    using Unity.Entities;

    public struct IncludeTag : IComponentData
    {
        public FixedString64Bytes Value;

        public class _ : ITriggerConditionActionConverter
        {
            public string Tag;

            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity) { ecb.AddComponent(index, entity, new IncludeTag() { Value = this.Tag }); }
        }
    }
}