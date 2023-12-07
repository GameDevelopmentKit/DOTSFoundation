namespace GASCore.Systems.EntityGeneratorSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.EntityGeneratorSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Transforms;
    using UnityEngine;


    [UpdateInGroup(typeof(GameAbilityBeginSimulationSystemGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct SetScaleByCastRangeSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new SetScaleByCastRangeJob()
            {
                CastRangeLookup = SystemAPI.GetComponentLookup<CastRangeComponent>(true)
            }.ScheduleParallel();
        }
    }

    [WithAll(typeof(ScaleByCastRangeTag))]
    [WithChangeFilter(typeof(ScaleByCastRangeTag))]
    [BurstCompile]
    public partial struct SetScaleByCastRangeJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<CastRangeComponent> CastRangeLookup;
        private void Execute(ref LocalTransform localTransform, in ActivatedStateEntityOwner activatedStateEntityOwner)
        {
            localTransform.Scale = this.CastRangeLookup[activatedStateEntityOwner].Value * 2;
        }
    }
}