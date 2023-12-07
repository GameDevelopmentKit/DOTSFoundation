namespace GASCore.Systems.TimelineSystems.Components
{
    using GASCore.Interfaces;
    using Unity.Entities;

    //todo should be trigger another trigger
    public struct TriggerByAnotherTrigger : IComponentData
    {
        public int  TriggerIndex;
        public bool IsCloneTarget;

        public class _ : ITimelineActionComponentConverter
        {
            public int  TriggerIndex;
            public bool IsCloneTarget = true;
            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
            {
                ecb.AddComponent(index, entity, new TriggerByAnotherTrigger()
                {
                    TriggerIndex = this.TriggerIndex,
                    IsCloneTarget = this.IsCloneTarget
                });
            }
        }
    }

    public struct WaitToTrigger : IBufferElementData
    {
        public Entity TriggerEntity;
    }
}