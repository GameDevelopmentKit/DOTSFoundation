namespace GASCore.Systems.LogicEffectSystems.Components
{
    using DOTSCore.Extension;
    using GameFoundation.Scripts.Utilities.Extension;
    using GASCore.Interfaces;
    using GASCore.Services;
    using GASCore.Systems.AbilityMainFlow.Factories;
    using GASCore.Systems.VisualEffectSystems.Components;
    using Unity.Entities;
    using Unity.Mathematics;
    using Zenject;

    public struct EntitySpawner : IComponentData
    {
        public Entity           EntityPrefab;
        public bool             IsDrop;
        public bool             FollowAffectedTargetRotation;
        public SimpleIntRange   AmountRange;
        public SimpleFloatRange StartAngleRange;
        public SimpleFloatRange AngleStepRange;

        public float CurrentAngle;
        public int   Clockwise;
    }

    public class EntitySpawnerAuthoring : IAbilityActionComponentConverter
    {
        [Inject] private AbilityActionEntityPrefabFactory actionEntityPrefabFactory;

        public bool             IsDrop               = false;
        public bool             FollowCasterRotation = false;
        public SimpleIntRange   AmountRange          = new() { min = 1, max = 1 };
        public SimpleFloatRange StartAngleRange      = new() { min = 0, max = 359 };
        public SimpleFloatRange AngleStepRange       = new() { min = 5, max = 5 };

        public EntityConverter.EntityData<IComponentConverter> EntityPrefab;

        public void Convert(EntityCommandBuffer.ParallelWriter ecb, int index, Entity entity)
        {
            ZenjectUtils.GetCurrentContainer()?.Inject(this);

            var entityPrefab = this.actionEntityPrefabFactory.CreateEntity(ecb, index, this.EntityPrefab.components);

            ecb.AddComponent(index, entity, new EntitySpawner()
            {
                EntityPrefab         = entityPrefab,
                IsDrop               = this.IsDrop,
                FollowAffectedTargetRotation = this.FollowCasterRotation,
                AmountRange          = this.AmountRange,
                StartAngleRange      = new SimpleFloatRange() { min = math.radians(this.StartAngleRange.min), max = math.radians(this.StartAngleRange.max) },
                AngleStepRange       = new SimpleFloatRange() { min = math.radians(this.AngleStepRange.min), max  = math.radians(this.AngleStepRange.max) },
                Clockwise            = 0,
            });
            ecb.SetParent(index, entityPrefab, entity);
        }
    }
}