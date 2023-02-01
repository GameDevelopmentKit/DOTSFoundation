namespace GASCore.Systems.LogicEffectSystems.Systems
{
    using DOTSCore.CommonSystems.Components;
    using GASCore.Groups;
    using GASCore.Systems.LogicEffectSystems.Components;
    using GASCore.Systems.StatSystems.Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [BurstCompile]
    public partial struct MovementEntityInDirJob : IJobEntity
    {
        public float DeltaTime;
        void Execute(ref Translation transform, in MovementDirection direction, in StatAspect statAspect)
        {
            if (direction.Value.Equals(float3.zero)) return;
            transform.Value +=  direction.Value * statAspect.GetCurrentValue(StatName.MovementSpeed) * this.DeltaTime;
        }
    }

    [UpdateInGroup(typeof(AbilityLogicEffectGroup))]
    [BurstCompile]
    public partial struct MovementInDirSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            new MovementEntityInDirJob() { DeltaTime = deltaTime }.ScheduleParallel();
        }
    }
}