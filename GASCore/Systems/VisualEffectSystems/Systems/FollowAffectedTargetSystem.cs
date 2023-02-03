namespace GASCore.Systems.VisualEffectSystems.Systems
{
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.VisualEffectSystems.Components;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    [UpdateInGroup(typeof(AbilityVisualEffectGroup))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct FollowAffectedTargetSystem : ISystem
    {
        private ComponentLookup<LocalToWorld> localToWorldLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state) { this.localToWorldLookup = state.GetComponentLookup<LocalToWorld>(true); }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            this.localToWorldLookup.Update(ref state);

            var deltaTime = SystemAPI.Time.DeltaTime;
            new FollowAffectedTargetJob
            {
                LocalToWorldLookup = this.localToWorldLookup,
                DeltaTime          = deltaTime,
            }.ScheduleParallel();
        }
    }

    [WithAll(typeof(FollowAffectedTarget))]
    [BurstCompile]
    public partial struct FollowAffectedTargetJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<LocalToWorld> LocalToWorldLookup;
        public            float                         DeltaTime;

        private void Execute(ref Translation translation, ref Rotation rotation, in FollowAffectedTarget data, in AffectedTargetComponent affectedTarget)
        {
            var targetPosition = this.LocalToWorldLookup[affectedTarget.Value].Position;

            if (data.LockAxis.x) targetPosition.x = translation.Value.x;
            if (data.LockAxis.y) targetPosition.y = translation.Value.y;
            if (data.LockAxis.z) targetPosition.z = translation.Value.z;

            if (math.distancesq(translation.Value, targetPosition) <= data.Radius * data.Radius) return;

            rotation.Value    =  quaternion.LookRotation(targetPosition - translation.Value, math.up());
            translation.Value += math.forward(rotation.Value) * data.MoveSpeed * this.DeltaTime;
        }
    }
}