namespace GASCore.Systems.EntityGeneratorSystems.Components
{
    using GASCore.Interfaces;
    using GASCore.Systems.VisualEffectSystems.Components;
    using Unity.Entities;
    using Unity.Mathematics;

    public struct PositionOffset : IComponentData
    {
        public float3 Value;
        
        public class _ : IAbilityActionComponentConverter
        {
            public SimpleVector3 Value;
            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
            {
                ecb.AddComponent(index, entity, new PositionOffset(){ Value = new float3(this.Value.x, this.Value.y, this.Value.z)});
            }
        }
    }
    
    public struct RandomPositionOffset : IComponentData
    {
        public float3 Min;
        public float3 Max;
        public class _ : IAbilityActionComponentConverter
        {
            public SimpleVector3 Min;
            public SimpleVector3 Max;
            public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
            {
                ecb.AddComponent(index, entity, new RandomPositionOffset()
                {
                    Min = this.Min,
                    Max = this.Max
                });
                
                ecb.AddComponent<PositionOffset>(index, entity);
            }
        }
    }
}