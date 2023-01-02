namespace GASCore.Interfaces
{
    using Unity.Entities;

    public interface IComponentConverter
    {
        public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity);
    }
}