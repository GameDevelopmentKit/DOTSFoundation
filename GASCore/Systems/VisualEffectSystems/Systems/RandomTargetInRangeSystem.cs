namespace GASCore.Systems.VisualEffectSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Interfaces;
    using GASCore.Systems.LogicEffectSystems.Components;
    using GASCore.UnityHybrid.Baker;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;
    using Random = Unity.Mathematics.Random;

    public struct RandomTargetPositionInRange : IComponentData, IAbilityActionComponentConverter
    {
        public float Range;
        public void  Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity) { ecb.AddComponent(index, entity, this); }
    }

    [UpdateInGroup(typeof(AbilityVisualEffectGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct RandomTargetInRangeSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { state.RequireForUpdate<RandomTargetPositionInRange>(); }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<BeginPresentationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            var worldBorder  = SystemAPI.GetSingleton<WorldBorderComponent>();
            var random       = new Random((uint)(SystemAPI.Time.ElapsedTime * 100000));
            var lifeTimeJob = new RandomTargetInRangeJob()
            {
                Ecb            = ecb,
                WorldBorder    = worldBorder,
                Random         = random
            };
            lifeTimeJob.ScheduleParallel();
        }
    }

    [BurstCompile]
    [WithChangeFilter(typeof(RandomTargetPositionInRange))]
    public partial struct RandomTargetInRangeJob : IJobEntity
    {
        public            EntityCommandBuffer.ParallelWriter Ecb;
        public            WorldBorderComponent               WorldBorder;
        public            Random                             Random;
        void Execute(Entity entity,[EntityIndexInQuery] int entityInQueryIndex, in RandomTargetPositionInRange moveToRandomTargetInRange, in LocalTransform transform)
        {
            float3 currentPos = transform.Position;
            float  x          = moveToRandomTargetInRange.Range * (-1);
            float  posz       = currentPos.z + Random.NextFloat(x, moveToRandomTargetInRange.Range);
            float  posx       = currentPos.x + Random.NextFloat(x, moveToRandomTargetInRange.Range);
            posx = math.clamp(posx, this.WorldBorder.Left, this.WorldBorder.Right);
            posz = math.clamp(posz, this.WorldBorder.Back, this.WorldBorder.Front);
            float3 position = new float3(posx, currentPos.y, posz);
            this.Ecb.AddComponent(entityInQueryIndex, entity, new TargetPosition(position, 0.5f));
        }
    }
}