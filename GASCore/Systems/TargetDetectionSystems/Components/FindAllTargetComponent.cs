namespace GASCore.Systems.TargetDetectionSystems.Components
{
    using Unity.Entities;


    public struct FindAllTargetTag : IComponentData { }
    
    public struct OverrideFindAllTargetTag : IComponentData { }

    public class FindAllTargetAuthoring : BaseFindTargetAuthoring
    {
        public override void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
        {
            base.Convert(ecb, index, entity);
            ecb.AddComponent(index, entity, new FindAllTargetTag());
        }
    }
}