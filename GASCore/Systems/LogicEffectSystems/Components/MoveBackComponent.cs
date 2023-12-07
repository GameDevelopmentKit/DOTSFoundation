namespace GASCore.Systems.LogicEffectSystems.Components
{
    using GASCore.Interfaces;
    using Unity.Entities;

    public struct IgnoreKnockBackTag : IComponentData
    {
        
    }

    public class IgnoreKnockBackTagAuthoring : IAbilityActionComponentConverter
    {
        public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
        {
            ecb.AddComponent(index,entity,new IgnoreKnockBackTag());
        }
    }
        

    public struct MoveBackComponent : IComponentData
    {
        public float PushBackForce;
    }

    public class MoveBackComponentAuthoring : IAbilityActionComponentConverter
    {
        public float pushBackForce;

        public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
        {
            ecb.AddComponent(index, entity, new MoveBackComponent() { PushBackForce = this.pushBackForce });
        }
    }
}