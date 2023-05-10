namespace GASCore.Systems.LogicEffectSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.LogicEffectSystems.Components;
    using Unity.Burst;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityVisualEffectGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct LifeTimeSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { state.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<LifeTime>().WithNone<EndTimeComponent>().Build()); }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton       = SystemAPI.GetSingleton<BeginPresentationEntityCommandBufferSystem.Singleton>();
            var ecb                = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            var currentElapsedTime = SystemAPI.Time.ElapsedTime;

            var lifeTimeJob = new LifeTimeJob()
            {
                Ecb                = ecb,
                CurrentElapsedTime = currentElapsedTime
            };
            lifeTimeJob.ScheduleParallel();
        }
    }

    [BurstCompile]
    [WithChangeFilter(typeof(LifeTime))]
    [WithNone(typeof(EndTimeComponent))]
    public partial struct LifeTimeJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;
        public double                             CurrentElapsedTime;
        void Execute(Entity entity, [EntityIndexInQuery] int entityInQueryIndex, in LifeTime lifeTime)
        {
            if (lifeTime.Value > 0)
            {
                this.Ecb.AddComponent(entityInQueryIndex, entity, new EndTimeComponent() { Value = this.CurrentElapsedTime + lifeTime.Value });
            }
        }
    }
}