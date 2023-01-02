namespace GASCore.Systems.TimelineSystems.Components
{
    using GASCore.Interfaces;
    using Unity.Entities;

    public struct TriggerAfterSecond : IComponentData
    {
        public float Second;

        public class _ : ITriggerConditionActionConverter
        {
            public float Second;

            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
            {
                ecb.AddComponent(index, entity, new TriggerAfterSecond()
                {
                    Second = this.Second ,
                });
            }
        }
    }
}