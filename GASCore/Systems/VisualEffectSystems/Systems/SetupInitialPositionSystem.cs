namespace GASCore.Systems.VisualEffectSystems.Systems
{
    using DOTSCore.CommonSystems.Components;
    using DOTSCore.Extension;
    using GASCore.Groups;
    using GASCore.Systems.AbilityMainFlow.Components;
    using GASCore.Systems.VisualEffectSystems.Components;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;
    using UnityEngine;
    using Random = Unity.Mathematics.Random;

    [UpdateInGroup(typeof(AbilityVisualEffectGroup))]
    [RequireMatchingQueriesForUpdate]
    public partial class SetupInitialPositionSystem : SystemBase
    {
        private AbilityPresentEntityCommandBufferSystem beginSimEcbSystem;

        protected override void OnCreate() { this.beginSimEcbSystem = this.World.GetExistingSystemManaged<AbilityPresentEntityCommandBufferSystem>(); }
        protected override void OnUpdate()
        {
            var ecb = this.beginSimEcbSystem.CreateCommandBuffer().AsParallelWriter();
            Entities.WithAny<AttachToAffectedTarget, PositionOffset, RandomPositionOffset>().WithNone<GameObjectHybridLink>().WithChangeFilter<AffectedTargetComponent>().ForEach(
                (Entity actionEntity, int entityInQueryIndex, ref Translation transform, in LocalToWorld localToWorld, in AffectedTargetComponent affectedTarget) =>
                {
                    if (HasComponent<AttachToAffectedTarget>(actionEntity))
                    {
                        transform.Value = float3.zero;
                        ecb.SetParent(entityInQueryIndex, actionEntity, affectedTarget.Value);
                    }
                    else
                    {
                        transform.Value = localToWorld.Position;
                    }

                    if (HasComponent<PositionOffset>(actionEntity))
                    {
                        var offsetPos = GetComponent<PositionOffset>(actionEntity);
                        transform.Value += new float3(offsetPos.Value.x * localToWorld.Forward.x, offsetPos.Value.y, offsetPos.Value.z * localToWorld.Forward.z);
                    }

                    if (HasComponent<RandomPositionOffset>(actionEntity))
                    {
                        var rnd                  = Random.CreateFromIndex((uint)(entityInQueryIndex + 1));
                        var randomPositionOffset = GetComponent<RandomPositionOffset>(actionEntity);
                        transform.Value += rnd.NextFloat3(randomPositionOffset.Min, randomPositionOffset.Max);
                    }
                }).ScheduleParallel();
            this.beginSimEcbSystem.AddJobHandleForProducer(this.Dependency);
        }
    }
}