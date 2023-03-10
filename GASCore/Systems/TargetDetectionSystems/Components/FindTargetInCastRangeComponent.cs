namespace GASCore.Systems.TargetDetectionSystems.Components
{
    using GASCore.Interfaces;
    using Sirenix.OdinInspector;
    using Unity.Collections;
    using Unity.Entities;

    public struct FindTargetInCastRangeComponent : IComponentData
    {
        public bool               IncludeTag;
        public FixedString64Bytes Tag;

        public class _ : ITriggerConditionActionConverter
        {
            public                        bool   IncludeTag;
            [ShowIf("IncludeTag")] public string Tag;

            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
            {
                ecb.AddComponent(index, entity, new FindTargetInCastRangeComponent
                {
                    IncludeTag = this.IncludeTag,
                    Tag        = this.Tag,
                });
            }
        }
    }
}