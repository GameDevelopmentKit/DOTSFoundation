namespace GASCore.Systems.VisualEffectSystems.Components
{
    using GASCore.Interfaces;
    using Unity.Entities;

    public struct MoveToAffectedTarget : IComponentData
    {
        public float RotateSpeed;
        
        public class _ : IAbilityActionComponentConverter
        {
            public float       RotateSpeed  = 5f;
            
            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
            {
                ecb.AddComponent(index, entity, new MoveToAffectedTarget(){RotateSpeed = this.RotateSpeed});
            }
        }
    }
}