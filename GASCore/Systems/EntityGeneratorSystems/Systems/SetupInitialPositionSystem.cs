namespace GASCore.Systems.EntityGeneratorSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.EntityGeneratorSystems.Components;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Transforms;
    using Random = Unity.Mathematics.Random;

    [UpdateInGroup(typeof(GameAbilityBeginSimulationSystemGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct SetupInitialPositionSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new RandomPositionOffsetJob() { RandomSeed = (uint)SystemAPI.Time.ElapsedTime * 100000 }.ScheduleParallel(state.Dependency);
            state.Dependency = new PositionOffsetJob().ScheduleParallel(state.Dependency);
        }
    }

    [WithChangeFilter(typeof(RandomPositionOffset))]
    [BurstCompile]
    public partial struct RandomPositionOffsetJob : IJobEntity
    {
        public uint RandomSeed;
        private void Execute([EntityIndexInQuery] int index, in RandomPositionOffset randomPositionOffset, ref PositionOffset positionOffset)
        {
            var rnd = Random.CreateFromIndex((uint)(this.RandomSeed + index));
            positionOffset.Value = rnd.NextFloat3(randomPositionOffset.Min, randomPositionOffset.Max);
        }
    }

    [WithNone(typeof(AttachPositionToAffectedTarget))]
    [WithChangeFilter(typeof(PositionOffset))]
    [BurstCompile]
    public partial struct PositionOffsetJob : IJobEntity
    {
        private void Execute(in PositionOffset positionOffset, ref LocalTransform localTransform) { localTransform.Position += positionOffset.Value; }
    }
}