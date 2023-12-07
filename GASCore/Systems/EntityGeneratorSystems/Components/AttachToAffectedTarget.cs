namespace GASCore.Systems.EntityGeneratorSystems.Components
{
    using GASCore.Interfaces;
    using Sirenix.OdinInspector;
    using Unity.Entities;

    public struct AttachToAffectedTarget : IComponentData
    {
        public class _ : IAbilityActionComponentConverter
        {
            [ReadOnly] public bool Position = true;
            public bool Rotation = true;
            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
            {
                if (!this.Position) return;
                if (this.Rotation)
                    ecb.AddComponent<AttachToAffectedTarget>(index, entity);
                else
                    ecb.AddComponent<AttachPositionToAffectedTarget>(index, entity);
            }
        }
    }

    public struct AttachPositionToAffectedTarget : IComponentData { }
}