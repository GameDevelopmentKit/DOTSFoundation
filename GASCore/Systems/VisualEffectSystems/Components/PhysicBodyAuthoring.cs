#if UNITY_PHYSICS_CUSTOM
namespace GASCore.Systems.VisualEffectSystems.Components
{
    using DOTSCore.UnityPhysicExtension.Utils;
    using GASCore.Interfaces;
    using Unity.Entities;
    using Unity.Physics.Authoring;
    
    public class PhysicBodyAuthoring : IAbilityActionComponentConverter
    {
        public BodyMotionType BodyMotionType;
        public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
        {
            ecb.AddPhysicBody(index, entity, new PhysicBodyData() { MotionType = this.BodyMotionType });
        }
    }
}
#endif