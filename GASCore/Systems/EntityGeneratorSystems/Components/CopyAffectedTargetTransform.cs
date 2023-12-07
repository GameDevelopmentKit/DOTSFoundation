namespace GASCore.Systems.EntityGeneratorSystems.Components
{
    using GASCore.Interfaces;
    using Unity.Entities;

    public struct CopyAffectedTargetTransform : IComponentData, IAbilityActionComponentConverter
    {
        public bool Position;
        public bool Rotation;
        public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
        {
            ecb.AddComponent(index, entity, new CopyAffectedTargetTransform()
            {
                Position = this.Position,
                Rotation = this.Rotation,
            });
        }
    }
}