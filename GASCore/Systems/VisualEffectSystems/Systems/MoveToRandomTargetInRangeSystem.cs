namespace GASCore.Systems.VisualEffectSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.LogicEffectSystems.Components;
    using GASCore.Systems.VisualEffectSystems.Components;
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
            var lifeTimeJob = new MoveToRandomTargetJob()
            {
                Ecb            = ecb,
                PositionLookup = SystemAPI.GetComponentLookup<LocalTransform>(true)
            };
            lifeTimeJob.ScheduleParallel();
        }
    }

    [BurstCompile]
    [WithAll(typeof(MoveToRandomTargetInRange))]
    [WithChangeFilter(typeof(MoveToRandomTargetInRange))]
    public partial struct MoveToRandomTargetJob : IJobEntity
    {
        public                         EntityCommandBuffer.ParallelWriter Ecb;
        [ReadOnly]             public  ComponentLookup<LocalTransform>    PositionLookup;
        [NativeSetThreadIndex] private int                                threadId;
        void Execute([EntityIndexInQuery] int entityInQueryIndex, in MoveToRandomTargetInRange moveToRandomTargetInRange, in AffectedTargetComponent affectedTargetComponent)
        {
            var    random     = Random.CreateFromIndex((uint)this.threadId);
            float3 currentPos = this.PositionLookup[affectedTargetComponent.Value].Position;
            float  x          = moveToRandomTargetInRange.Range * (-1);
            float  posx       = currentPos.x + random.NextFloat(x, moveToRandomTargetInRange.Range);
            float  posz       = currentPos.z + random.NextFloat(x, moveToRandomTargetInRange.Range);
            float3 position   = new float3(posx, currentPos.y, posz);
            this.Ecb.AddComponent(entityInQueryIndex, affectedTargetComponent.Value, new TargetPosition(position, 0.5f));
        }
    }
}