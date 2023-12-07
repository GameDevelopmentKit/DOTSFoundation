namespace GASCore.Systems.VisualEffectSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using GASCore.Systems.LogicEffectSystems.Systems;
    using GASCore.Systems.VisualEffectSystems.Components;
    using GASCore.UnityHybrid.Baker;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;
    using Random = Unity.Mathematics.Random;

    [UpdateInGroup(typeof(AbilityVisualEffectGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct MoveToRandomTargetInRangeSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { state.RequireForUpdate<MoveToRandomTargetInRange>(); }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<BeginPresentationEntityCommandBufferSystem.Singleton>();
            var ecb          = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            var worldBorder  = SystemAPI.GetSingleton<WorldBorderComponent>();
            var random       = new Random((uint)(SystemAPI.Time.ElapsedTime * 100000));
            var lifeTimeJob = new MoveToRandomTargetJob()
            {
                Ecb            = ecb,
                PositionLookup = SystemAPI.GetComponentLookup<LocalTransform>(true),
                WorldBorder    = worldBorder,
                Random = random
            };
            lifeTimeJob.ScheduleParallel();
        }
    }

    [BurstCompile]
    [WithAll(typeof(MoveToRandomTargetInRange))]
    [WithChangeFilter(typeof(MoveToRandomTargetInRange))]
    public partial struct MoveToRandomTargetJob : IJobEntity
    {
        public            EntityCommandBuffer.ParallelWriter Ecb;
        public            WorldBorderComponent               WorldBorder;
        public            Random                             Random;
        [ReadOnly] public ComponentLookup<LocalTransform>    PositionLookup;
        void Execute([EntityIndexInQuery] int entityInQueryIndex, in MoveToRandomTargetInRange moveToRandomTargetInRange, in AffectedTargetComponent affectedTargetComponent)
        {
            float3 currentPos = this.PositionLookup[affectedTargetComponent.Value].Position;
            float  x          = moveToRandomTargetInRange.Range * (-1);
            float  posz       = currentPos.z + Random.NextFloat(x, moveToRandomTargetInRange.Range);
            float  posx       = currentPos.x + Random.NextFloat(x, moveToRandomTargetInRange.Range);
            posx = math.clamp(posx, this.WorldBorder.Left, this.WorldBorder.Right);
            posz = math.clamp(posz, this.WorldBorder.Back, this.WorldBorder.Front);
            float3 position = new float3(posx, currentPos.y, posz);
            this.Ecb.AddComponent(entityInQueryIndex, affectedTargetComponent.Value, new TargetPosition(position, 0.5f));
            this.Ecb.AddComponent<MoveTowardTarget>(entityInQueryIndex,  affectedTargetComponent.Value);
        }
    }
}