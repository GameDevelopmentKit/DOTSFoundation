namespace GASCore.Systems.LogicEffectSystems.Components
{
    using GASCore.Interfaces;
    using Sirenix.OdinInspector;
    using Unity.Collections;
    using Unity.Entities;

    public struct FindNearestTarget : IComponentData
    {
        public bool               IsIncludedTag;
        public FixedString64Bytes TargetTag;
        public class Tag : ITriggerConditionActionConverter
        {
            public bool IsIncludedTag;
            [ShowIf("IsIncludedTag")]
            public string TargetTag;
            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
            {
                ecb.AddComponent(index, entity, new FindNearestTarget()
                {
                    IsIncludedTag = this.IsIncludedTag,
                    TargetTag = this.TargetTag
                });
            }
        }
    }
}